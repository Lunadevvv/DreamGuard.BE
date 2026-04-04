using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<PaginatedList<Product>> GetAllProductByCategoryAsync(int cateId, int pageNumber, decimal? maxPrice, string? color, int? maxAgeGroup);
        Task<PaginatedList<Product>> GetAllProductToTradeInAsync(int? cateId, int pageNumber, int pageSize, decimal? maxPrice, string? color, int? maxAgeGroup);
        Task<PaginatedList<Product>> GetAllProductsForAdminAsync(int pageNumber, string? name);
        Task<Product?> GetProductBySlugAsync(string slug);
        Task<Product?> GetProductByIdAsync(Guid id);
        Task<Product?> GetProductByIdForUpdateAsync(Guid id);
        Task<List<Product>> GetFullyCustomizedProductsAsync();
        Task UpdateProductCertificatesAsync(Product product, List<ProductCertificate> certificates);
    }
}