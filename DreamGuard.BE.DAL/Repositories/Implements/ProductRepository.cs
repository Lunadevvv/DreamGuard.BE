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

        public async Task<PaginatedList<Product>> GetAllProductByCategoryAsync(int cateId, int pageNumber, double? maxPrice, string? color, int? maxAgeGroup)
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
    }
}