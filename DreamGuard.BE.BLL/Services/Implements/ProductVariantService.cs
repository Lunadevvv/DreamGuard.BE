using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly IProductVariantRepository _variantRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public ProductVariantService(
            IProductVariantRepository variantRepository,
            IProductRepository productRepository,
            IMapper mapper)
        {
            _variantRepository = variantRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<ProductVariantResponse>>> GetVariantsByProductIdAsync(Guid productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return Result<List<ProductVariantResponse>>.Failure("Product not found.", 404);
            }

            var variants = await _variantRepository.GetVariantsByProductIdAsync(productId);

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

            var variant = _mapper.Map<ProductVariant>(request);
            variant.Id = Guid.NewGuid();
            variant.IsActive = true;
            variant.CreatedAt = DateTime.UtcNow;

            var res = await _variantRepository.CreateAsync(variant);
            if (res < 0)
            {
                return Result<ProductVariantResponse>.Failure("Failed to create variant.", 400);
            }

            return Result<ProductVariantResponse>.Success(MapToResponse(variant));
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

            _mapper.Map(request, variant);

            var res = await _variantRepository.UpdateAsync(variant);
            if (res < 0)
            {
                return Result<ProductVariantResponse>.Failure("Failed to update variant.", 400);
            }

            return Result<ProductVariantResponse>.Success(MapToResponse(variant));
        }

        public async Task<Result<bool>> UpdateVariantStatusAsync(Guid id, bool isActive)
        {
            var variant = await _variantRepository.GetVariantByIdAsync(id);
            if (variant == null)
            {
                return Result<bool>.Failure("Variant not found.", 404);
            }

            variant.IsActive = isActive;

            var res = await _variantRepository.UpdateAsync(variant);
            if (res < 0)
            {
                return Result<bool>.Failure("Failed to update variant status.", 400);
            }

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeleteVariantAsync(Guid id)
        {
            var variant = await _variantRepository.GetVariantByIdAsync(id);
            if (variant == null)
            {
                return Result<bool>.Failure("Variant not found.", 404);
            }

            var res = await _variantRepository.RemoveAsync(variant);
            if (!res)
            {
                return Result<bool>.Failure("Failed to delete variant.", 400);
            }

            return Result<bool>.Success(true);
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
                IsNew = v.IsNew,
                IsActive = v.IsActive,
                CreatedAt = v.CreatedAt,
                ProductId = v.ProductId
            };
        }
    }
}
