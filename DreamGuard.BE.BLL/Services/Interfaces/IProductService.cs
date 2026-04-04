using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IProductService
    {
        Task<Result<PaginatedList<ProductResponse>>> GetAllProductByCategoryAsync(int cateId, int pageNumber, decimal? maxPrice, string? color, int? maxAgeGroup);
        Task<Result<PaginatedList<ProductResponse>>> GetAllProductToTradeInAsync(int? cateId, int pageNumber, int pageSize, decimal? maxPrice, string? color, int? maxAgeGroup);
        Task<Result<PaginatedList<ProductResponseForAdmin>>> GetAllProductsForAdminAsync(int pageNumber, string? name);
        Task<Result<ProductDetailResponse>> GetProductDetailBySlugAsync(string slug);
        Task<Result<Product>> GetProductByIdAsync(Guid id);
        Task<Result<bool>> CreateProductAsync(CreateProductRequest product);
        Task<Result<bool>> UpdateProductAsync(UpdateProductRequest product);
        Task<Result<bool>> UpdateProductStatusAsync(Guid id, ProductStatus status);
        Task<Result<ProductDetailResponse>> CreateFullyCustomizeProductAsync(CreateFullyCustomizeProductRequest request);
        Task<Result<List<ProductResponse>>> GetFullyCustomizedProductsAsync();
    }
}