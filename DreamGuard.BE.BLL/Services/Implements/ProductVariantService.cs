using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly IProductVariantRepository _variantRepository;
        private readonly IProductRepository _productRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductVariantService(
            IProductVariantRepository variantRepository,
            IProductRepository productRepository,
            IInventoryRepository inventoryRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _variantRepository = variantRepository;
            _productRepository = productRepository;
            _inventoryRepository = inventoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<List<ProductVariantResponse>>> GetVariantsByProductIdAsync(Guid productId, string? size, string? color)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return Result<List<ProductVariantResponse>>.Failure("Product not found.", 404);
            }

            var variants = await _variantRepository.GetVariantsByProductIdAsync(productId, size, color);

            var response = variants.Select(MapToResponse).ToList();

            return Result<List<ProductVariantResponse>>.Success(response);
        }

        public async Task<Result<ProductVariantResponse>> GetVariantByIdAsync(Guid id)
        {
            var variant = await _variantRepository.GetVariantByIdAsync(id);
            if (variant == null)
            {
                return Result<ProductVariantResponse>.Failure("Variant not found.", 404);
            }

            return Result<ProductVariantResponse>.Success(MapToResponse(variant));
        }

        public async Task<Result<ProductVariantAdminResponse>> GetVariantsByProductIdForAdminAsync(Guid productId)
        {
            var variants = await _variantRepository.GetVariantsByProductIdForAdminAsync(productId);

            // Nhóm theo Color
            var colorGroups = variants
                .GroupBy(v => v.Attributes?.Color ?? "Unknown")
                .Select(group => new ProductVariantGroupResponse
                {
                    Color = group.Key,
                    Variants = group.Select(v => new ProductVariantItemResponse
                    {
                        Id = v.Id,
                        Size = v.Size ?? string.Empty,
                        Sku = v.Sku ?? string.Empty,
                        SalePrice = v.SalePrice,
                        BasePrice = v.BasePrice,
                        StockQuantity = v.Inventory?.Quantity ?? 0,
                        StockStatus = GetStockStatus(v.Inventory?.Quantity ?? 0, v.Inventory?.LowStockThreshold ?? 10),
                        Status = v.Status.ToString()
                    })
                    .ToList()
                })
                .ToList();

            var res = new ProductVariantAdminResponse
            {
                ProductId = productId,
                TotalVariants = variants.Count,
                ColorGroups = colorGroups
            };

            return Result<ProductVariantAdminResponse>.Success(res);
        }

        public async Task<Result<ProductVariantResponse>> CreateVariantAsync(CreateProductVariantRequest request)
        {
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null)
            {
                return Result<ProductVariantResponse>.Failure("Product not found.", 404);
            }

            if (request.SalePrice > request.BasePrice)
            {
                return Result<ProductVariantResponse>.Failure(
                    "Sale price cannot be greater than base price.", 400);
            }

            if (await _variantRepository.IsVariantSkuUniqueAsync(request.Sku))
            {
                return Result<ProductVariantResponse>.Failure("SKU must be unique.", 400);
            }

            var variant = _mapper.Map<ProductVariant>(request);
            variant.Id = Guid.NewGuid();
            variant.Status = ProductStatus.Draft;
            variant.CreatedAt = DateTime.UtcNow;
            variant.Size = GenerateSize(variant.Attributes);

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var variantResult = await _variantRepository.CreateAsync(variant);
                if (variantResult < 0)
                {
                    await transaction.RollbackAsync();
                    return Result<ProductVariantResponse>.Failure("Failed to create variant.", 400);
                }

                var inventory = new Inventory
                {
                    Id = Guid.NewGuid(),
                    Quantity = 0,
                    LowStockThreshold = 10,
                    UpdatedAt = DateTime.UtcNow,
                    ProductVariantId = variant.Id
                };

                var inventoryResult = await _inventoryRepository.CreateAsync(inventory);
                if (inventoryResult < 0)
                {
                    await transaction.RollbackAsync();
                    return Result<ProductVariantResponse>.Failure("Failed to create inventory for variant.", 400);
                }

                await transaction.CommitAsync();
                return Result<ProductVariantResponse>.Success(MapToResponse(variant));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Result<ProductVariantResponse>.Failure("Failed to create variant with inventory.", 500);
            }
        }

        public async Task<Result<ProductVariantResponse>> UpdateVariantAsync(Guid id, UpdateProductVariantRequest request)
        {
            var variant = await _variantRepository.GetVariantByIdAsync(id);
            if (variant == null)
            {
                return Result<ProductVariantResponse>.Failure("Variant not found.", 404);
            }

            if (request.SalePrice > request.BasePrice)
            {
                return Result<ProductVariantResponse>.Failure(
                    "Sale price cannot be greater than base price.", 400);
            }

            if (await _variantRepository.IsVariantSkuUniqueAsync(request.Sku))
            {
                return Result<ProductVariantResponse>.Failure("SKU must be unique.", 400);
            }

            _mapper.Map(request, variant);
            variant.Size = GenerateSize(variant.Attributes);

            var res = await _variantRepository.UpdateAsync(variant);
            if (res < 0)
            {
                return Result<ProductVariantResponse>.Failure("Failed to update variant.", 400);
            }

            return Result<ProductVariantResponse>.Success(MapToResponse(variant));
        }

        public async Task<Result<bool>> UpdateVariantStatusAsync(Guid id, ProductStatus status)
        {
            var variant = await _variantRepository.GetVariantByIdAsync(id);
            if (variant == null)
            {
                return Result<bool>.Failure("Variant not found.", 404);
            }

            variant.Status = status;

            var res = await _variantRepository.UpdateAsync(variant);
            if (res < 0)
            {
                return Result<bool>.Failure("Failed to update variant status.", 400);
            }

            return Result<bool>.Success(true);
        }

        private static string GenerateSize(ProductAttribute? attributes)
        {
            if(attributes == null)
            {
                return string.Empty;
            }
            
            if (attributes.Length > 0 && attributes.Width > 0 && attributes.Thickness > 0)
            {
                return string.Format("{0}x{1}x{2}", attributes.Length, attributes.Width, attributes.Thickness);
            }

            if (attributes.Length > 0 && attributes.Width > 0 && attributes.Thickness == null)
            {
                return string.Format("{0}x{1}", attributes.Length, attributes.Width);
            }

            return string.Empty;
        }

        private static ProductVariantResponse MapToResponse(ProductVariant v)
        {
            return new ProductVariantResponse
            {
                Id = v.Id,
                Sku = v.Sku,
                BasePrice = v.BasePrice,
                SalePrice = v.SalePrice,
                Weight = v.Weight,
                Attributes = v.Attributes,
                Size = v.Size,
                IsNew = v.IsNew,
                Status = v.Status,
                CreatedAt = v.CreatedAt,
                ProductId = v.ProductId
            };
        }

        private static string GetStockStatus(int quantity, int lowStockThreshold)
        {
            if (quantity > 0)
            {
                return quantity <= lowStockThreshold ? "Low Stock" : "In Stock";
            }
            return "Out of Stock";
        }

    }
}
