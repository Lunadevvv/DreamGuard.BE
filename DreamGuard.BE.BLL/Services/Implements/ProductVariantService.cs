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
        private readonly IProductCustomizeTypeRepository _customizeTypeRepository;
        private readonly IVariantCustomizeTypeRepository _variantCustomizeTypeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductVariantService(
            IProductVariantRepository variantRepository,
            IProductRepository productRepository,
            IInventoryRepository inventoryRepository,
            IProductCustomizeTypeRepository customizeTypeRepository,
            IVariantCustomizeTypeRepository variantCustomizeTypeRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _variantRepository = variantRepository;
            _productRepository = productRepository;
            _inventoryRepository = inventoryRepository;
            _customizeTypeRepository = customizeTypeRepository;
            _variantCustomizeTypeRepository = variantCustomizeTypeRepository;
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

        public async Task<Result<ProductVariantResponse>> CreateVariantWithCustomizeAsync(CreateVariantWithCustomizeRequest request)
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

            // Batch-load all customize types at once to avoid N+1 queries
            var customizeTypeIds = request.CustomizeTypeIds?.Distinct().ToList() ?? new List<Guid>();
            List<ProductCustomizeType> customizeTypes = new();
            if (customizeTypeIds.Any())
            {
                customizeTypes = await _customizeTypeRepository.GetByIdsAsync(customizeTypeIds);
                var notFoundIds = customizeTypeIds.Except(customizeTypes.Select(ct => ct.Id)).ToList();
                if (notFoundIds.Any())
                {
                    return Result<ProductVariantResponse>.Failure(
                        $"Customize types not found: {string.Join(", ", notFoundIds)}", 404);
                }

                // Validate all customize types are Active
                var inactiveTypes = customizeTypes.Where(ct => ct.Status != CustomizeTypeStatus.Active).ToList();
                if (inactiveTypes.Any())
                {
                    return Result<ProductVariantResponse>.Failure(
                        $"Customize types are not active: {string.Join(", ", inactiveTypes.Select(ct => ct.Name))}", 400);
                }
            }
            else
            {
                return Result<ProductVariantResponse>.Failure("At least one customize type must be provided.", 400);
            }

            var variant = _mapper.Map<ProductVariant>(request);
            variant.Id = Guid.NewGuid();
            variant.Status = ProductStatus.Draft;
            variant.CreatedAt = DateTime.UtcNow;
            variant.Size = GenerateSize(variant.Attributes);
            variant.IsCustomizable = customizeTypes.Any();

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

                // Batch-create all VariantCustomizeType records at once (no N+1)
                if (customizeTypes.Any())
                {
                    var variantCustomizeTypes = customizeTypes.Select(ct => new VariantCustomizeType
                    {
                        CusId = ct.Id,
                        ProductVariantId = variant.Id,
                        OverridePrice = ct.DefaultPrice
                    }).ToList();

                    await _variantCustomizeTypeRepository.AddRangeAsync(variantCustomizeTypes);

                    // Populate navigation properties for the response
                    variant.VariantCustomizeTypes = variantCustomizeTypes;
                    foreach (var vct in variant.VariantCustomizeTypes)
                    {
                        vct.ProductCustomizeType = customizeTypes.First(ct => ct.Id == vct.CusId);
                    }
                }

                variant.Inventory = inventory;
                await transaction.CommitAsync();
                return Result<ProductVariantResponse>.Success(MapToResponse(variant));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Result<ProductVariantResponse>.Failure("Failed to create variant with customize options.", 500);
            }
        }

        public async Task<Result> AssignCustomizeTypeAsync(Guid variantId, AssignCustomizeTypeRequest request)
        {
            var variant = await _variantRepository.GetVariantByIdAsync(variantId);
            if (variant == null)
            {
                return Result.Failure("Variant not found.", 404);
            }
            
            if(variant.IsCustomizable == false)
            {
                return Result.Failure("Variant is not customizable.", 400);
            }

            var customizeType = await _customizeTypeRepository.GetByIdAsync(request.CustomizeTypeId);
            if (customizeType == null)
            {
                return Result.Failure("Customize type not found.", 404);
            }

            if (customizeType.Status != CustomizeTypeStatus.Active)
            {
                return Result.Failure("Customize type is not active.", 400);
            }

            // Check if already assigned
            var existing = await _variantCustomizeTypeRepository.GetByCompositeKeyAsync(request.CustomizeTypeId, variantId);
            if (existing != null)
            {
                return Result.Failure("Customize type is already assigned to this variant.", 400);
            }

            var variantCustomizeType = new VariantCustomizeType
            {
                CusId = request.CustomizeTypeId,
                ProductVariantId = variantId,
                OverridePrice = request.OverridePrice > 0 ? request.OverridePrice : customizeType.DefaultPrice
            };

            var result = await _variantCustomizeTypeRepository.CreateAsync(variantCustomizeType);
            if (result < 0)
            {
                return Result.Failure("Failed to assign customize type.", 400);
            }

            return Result.Success($"Customize type '{customizeType.Name}' assigned to variant successfully.");
        }

        public async Task<Result> RemoveCustomizeTypeAsync(Guid variantId, Guid customizeTypeId)
        {
            var existing = await _variantCustomizeTypeRepository.GetByCompositeKeyAsync(customizeTypeId, variantId);
            if (existing == null)
            {
                return Result.Failure("Customize type is not assigned to this variant.", 404);
            }

            var result = await _variantCustomizeTypeRepository.RemoveAsync(existing);
            if (!result)
            {
                return Result.Failure("Failed to remove customize type.", 400);
            }

            return Result.Success("Customize type removed from variant successfully.");
        }

        public async Task<Result> UpdateCustomizeTypePriceAsync(Guid variantId, Guid customizeTypeId, UpdateCustomizeTypePriceRequest request)
        {
            var existing = await _variantCustomizeTypeRepository.GetByCompositeKeyAsync(customizeTypeId, variantId);
            if (existing == null)
            {
                return Result.Failure("Customize type is not assigned to this variant.", 404);
            }

            existing.OverridePrice = request.OverridePrice;
            var result = await _variantCustomizeTypeRepository.UpdateAsync(existing);
            if (result < 0)
            {
                return Result.Failure("Failed to update customize type price.", 400);
            }

            return Result.Success($"Customize type price updated to {request.OverridePrice} successfully.");
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

            if (await _variantRepository.IsVariantSkuUniqueAsync(request.Sku, id))
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

            //check stock
            if (variant.Inventory != null)
            {
                if ((status == ProductStatus.Published || status == ProductStatus.Hidden) && variant.Inventory.Quantity <= 0)
                {
                    return Result<bool>.Failure("Cannot activate or hide variant with zero stock.", 400);
                }
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
                IsCustomizable = v.IsCustomizable,
                Status = v.Status,
                CreatedAt = v.CreatedAt,
                ProductId = v.ProductId,
                StockQuantity = v.Inventory?.Quantity ?? 0,
                StockStatus = GetStockStatus(v.Inventory?.Quantity ?? 0, v.Inventory?.LowStockThreshold ?? 10),
                CustomizeOptions = v.VariantCustomizeTypes?.Select(vct => new CustomizeOptionResponse
                {
                    CustomizeTypeId = vct.CusId,
                    Name = vct.ProductCustomizeType?.Name ?? string.Empty,
                    Summary = vct.ProductCustomizeType?.Summary ?? string.Empty,
                    DefaultPrice = vct.ProductCustomizeType?.DefaultPrice ?? 0,
                    OverridePrice = vct.OverridePrice
                }).ToList() ?? new List<CustomizeOptionResponse>()
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
