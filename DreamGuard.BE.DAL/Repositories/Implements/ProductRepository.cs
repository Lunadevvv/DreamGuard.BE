using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;
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

        public async Task<PaginatedList<Product>> GetAllProductByCategoryAsync(int cateId, int pageNumber)
        {
            var query = _context.Products
                    .Include(p => p.Variants.Where(v => v.IsActive))
                    .Include(p => p.Assets)
                    .Where(p => p.CateId == cateId && p.IsActive)
                    .OrderByDescending(p => p.AverageRating)
                    .AsSplitQuery()
                    .AsNoTracking();
            return await PaginatedList<Product>.CreateAsync(query, pageNumber, 10);
        }

        public async Task<Product?> GetProductBySlugAsync(string slug)
        {
            return await _context.Products
                .Include(p => p.Variants.Where(v => v.IsActive))
                .Include(p => p.Assets)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);
        }

        public async Task<Product?> GetProductByIdAsync(Guid id)
        {
            return await _context.Products
                .Include(p => p.Assets)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        }
    }
}