using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<Result<bool>> CreateProductAsync(Product product)
        {
            var res = await _productRepository.CreateAsync(product);
            if (res < 0)
            {
                return Result<bool>.Failure("Failed to create product.", 400);
            }

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> UpdateProductAsync(Product product)
        {
            var res = await _productRepository.UpdateAsync(product);
            if (res < 0)
            {
                return Result<bool>.Failure("Failed to update product.", 400);
            }

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeleteProductAsync(Guid id)
        {
            var prod = await _productRepository.GetProductByIdAsync(id);
            if (prod == null)
            {
                return Result<bool>.Failure("Product not found.", 404);
            }

            // Soft delete by setting IsActive to false
            prod.IsActive = false;
            //Update product to db
            var res = await _productRepository.UpdateAsync(prod);
            if (res < 0)
            {
                return Result<bool>.Failure("Failed to delete product.", 400);
            }

            return Result<bool>.Success(true);
        }

        public async Task<Result<PaginatedList<ProductResponse>>> GetAllProductByCategoryAsync(int cateId, int pageNumber, double? maxPrice, string? color, int? maxAgeGroup)
        {
            var products = await _productRepository.GetAllProductByCategoryAsync(cateId, pageNumber, maxPrice, color, maxAgeGroup);

            //check if products is null or empty
            if (products == null || !products.Items.Any())
            {
                return Result<PaginatedList<ProductResponse>>.Failure("No products found for the given category.", 404);
            }

            //check if product variant is empty then set baseprice and saleprice to 0
            var basePrice = 0;
            var salePrice = 0;
            if(products.Items.Any(p =>p.Variants.Any()))
            {
                basePrice = (int)products.Items.Min(p => p.Variants.Min(v => v.BasePrice));
                salePrice = (int)products.Items.Min(p => p.Variants.Min(v => v.SalePrice));
            }

            // Map products to ProductResponse
            var productResponses = products.Items.Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Summary = p.Summary,
                Slug = p.Slug,
                AgeGroup = p.AgeGroup,
                AverageRating = p.AverageRating,
                BasePrice = basePrice,
                SalePrice = salePrice,
                ImageUrls = p.Assets.Select(a => a.Url).ToList()
            }).ToList();

            // Create a new PaginatedList for ProductResponse
            var paginatedResponse = new PaginatedList<ProductResponse>(
                productResponses,
                products.TotalCount,
                products.PageNumber,
                products.PageSize
            );

            return Result<PaginatedList<ProductResponse>>.Success(paginatedResponse);
        }

        public async Task<Result<Product>> GetProductByIdAsync(Guid id)
        {
            var prod = await _productRepository.GetProductByIdAsync(id);

            //check if product is null
            if (prod == null)
            {
                return Result<Product>.Failure("Product not found.", 404);
            }

            return Result<Product>.Success(prod);
        }

        public async Task<Result<ProductDetailResponse>> GetProductDetailBySlugAsync(string slug)
        {
            var prod = await _productRepository.GetProductBySlugAsync(slug);

            //check if product is null
            if (prod == null)
            {
                return Result<ProductDetailResponse>.Failure("Product not found.", 404);
            }

            // Map product to ProductDetailResponse
            var productDetailResponse = new ProductDetailResponse
            {
                Id = prod.Id,
                Name = prod.Name,
                Summary = prod.Summary,
                Description = prod.Description,
                AgeGroup = prod.AgeGroup,
                AverageRating = prod.AverageRating,
                WarrantyPolicyDay = prod.WarrantyPolicyDay,
                ReturnPolicyDay = prod.ReturnPolicyDay,
                Variants = prod.Variants.Select(v => new ProductVariantResponse
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
                }).ToList(),
                ImageUrls = prod.Assets.Select(a => a.Url).ToList()
            };

            return Result<ProductDetailResponse>.Success(productDetailResponse);
        }
    }
}