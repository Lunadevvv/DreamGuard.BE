using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.DbContext;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DreamGuard.BE.DAL.Repositories.Implements
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(DreamGuardContext context) : base(context) { }

        public async Task<PaginatedList<Product>> GetAllProductByCategoryAsync(int cateId, int pageNumber, decimal? maxPrice, string? color, int? maxAgeGroup)
        {
            var query = _context.Products
                    .Include(p => p.Variants.Where(v => v.Status != ProductStatus.Hidden && v.Status != ProductStatus.Draft))
                    .Include(p => p.Assets)
                    .Where(p => p.CateId == cateId && p.Status == ProductStatus.Published)
                    .OrderByDescending(p => p.AverageRating)
                    .AsSplitQuery()
                    .AsNoTracking();
                
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Variants.Any(v => v.SalePrice <= maxPrice.Value));
            }

            if (!string.IsNullOrEmpty(color))
            {
                query = query.Where(p => p.Variants.Any(v => v.Attributes.Color == color));
            }

            if (maxAgeGroup.HasValue)
            {
                query = query.Where(p => p.AgeGroup <= maxAgeGroup.Value);
            }
            return await PaginatedList<Product>.CreateAsync(query, pageNumber, 10);
        }
        public async Task<PaginatedList<Product>> GetAllProductToTradeInAsync(int? cateId, int pageNumber, int pageSize, decimal? maxPrice, string? color, int? maxAgeGroup)
        {
            var query = _context.Products
                    .Include(p => p.Variants.Where(v => v.Status != ProductStatus.Hidden && v.Status != ProductStatus.Draft))
                    .Include(p => p.Assets)
                    .Where(p => (cateId == null || p.CateId == cateId) && p.Status == ProductStatus.Published && p.IsTradeInEligible)
                    .OrderByDescending(p => p.AverageRating)
                    .AsNoTracking();
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Variants.Any(v => v.SalePrice <= maxPrice.Value));
            }

            if (!string.IsNullOrEmpty(color))
            {
                query = query.Where(p => p.Variants.Any(v => v.Attributes.Color == color));
            }

            if (maxAgeGroup.HasValue)
            {
                query = query.Where(p => p.AgeGroup <= maxAgeGroup.Value);
            }
            return await PaginatedList<Product>.CreateAsync(query, pageNumber, pageSize);
        }

        public async Task<Product?> GetProductBySlugAsync(string slug)
        {
            return await _context.Products
                .Include(p => p.Variants.Where(v => v.Status != ProductStatus.Draft && v.Status != ProductStatus.Hidden))
                .Include(p => p.Assets)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Slug == slug);
        }

        public async Task<Product?> GetProductByIdAsync(Guid id)
        {
            return await _context.Products
                .Include(p => p.Assets)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<PaginatedList<Product>> GetAllProductsForAdminAsync(int pageNumber, string? name)
        {
            var query = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Variants)
                    .Include(p => p.Assets)
                    .OrderByDescending(p => p.CreatedAt)
                    .AsSplitQuery()
                    .AsNoTracking();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(p => p.Name.Contains(name));
            }
            return await PaginatedList<Product>.CreateAsync(query, pageNumber, 10);
        }

        public async Task<Product?> GetProductByIdForUpdateAsync(Guid id)
        {
            return await _context.Products
                .Include(p => p.Certificates)
                .AsTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Product>> GetFullyCustomizedProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Variants.Where(v => v.Status != ProductStatus.Draft && v.Status != ProductStatus.Hidden))
                .Include(p => p.Assets)
                .Where(p => p.FullyCustomizedProductType != FullyCustomizedProductType.None && p.Status != ProductStatus.Draft && p.Status != ProductStatus.Hidden)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task UpdateProductCertificatesAsync(Product product, List<ProductCertificate> certificates)
        {
            var incomingCertIds = certificates.Select(c => c.Id).ToHashSet();

            // Xóa các certificate cũ không còn được chọn 
            foreach (var cert in product.Certificates.Where(c => !incomingCertIds.Contains(c.Id)).ToList())
            {
                product.Certificates.Remove(cert);
            }

            // Thêm các certificate mới được bổ sung
            foreach (var cert in certificates.Where(c => !product.Certificates.Any(ec => ec.Id == c.Id)))
            {
                var tracked = _context.ProductCertificates.Local.FirstOrDefault(c => c.Id == cert.Id);
                if (tracked == null)
                {
                    _context.ProductCertificates.Attach(cert);
                }
                product.Certificates.Add(tracked ?? cert);
            }
            
            await _context.SaveChangesAsync();
        }
    }
}