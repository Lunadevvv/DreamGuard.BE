using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.DbContext;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DreamGuard.BE.DAL.Repositories.Implements
{
    public class ProductVariantRepository : GenericRepository<ProductVariant>, IProductVariantRepository
    {
        public ProductVariantRepository(DreamGuardContext context) : base(context)
        {
        }

        public async Task<List<ProductVariant>> GetVariantsByProductIdAsync(Guid productId, string? size, string? color)
        {
            var query = await _context.ProductVariants
                .Where(v => v.ProductId == productId && v.Status != ProductStatus.Draft && v.Status != ProductStatus.Hidden)
                .OrderByDescending(v => v.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            if (!string.IsNullOrEmpty(size))
            {
                query = query.Where(v => v.Size == size).ToList();
            }

            if (!string.IsNullOrEmpty(color))
            {
                query = query.Where(v => v.Attributes != null && v.Attributes.Color == color).ToList();
            }

            return query;
        }

        public async Task<ProductVariant?> GetVariantByIdAsync(Guid id)
        {
            return await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<List<ProductVariant>> GetVariantsByProductIdForAdminAsync(Guid productId)
        {
            return await _context.ProductVariants
                .Include(v => v.Inventory)
                .Where(v => v.ProductId == productId)
                .OrderBy(v => v.Attributes != null ? v.Attributes.Color : string.Empty)
                .ThenBy(v => v.Size)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> IsVariantSkuUniqueAsync(string sku, Guid? excludeVariantId = null)
        {
            return await _context.ProductVariants
                .AnyAsync(v => v.Sku == sku && (!excludeVariantId.HasValue || v.Id != excludeVariantId.Value));
        }

        public async Task<ProductVariant?> GetVariantByIdForUpdateAsync(Guid id)
        {
            return await _context.ProductVariants
                .AsTracking()
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<List<ProductVariant>> GetVariantsByProductIdForStockCheckAsync(Guid productId)
        {
            return await _context.ProductVariants
                .Include(v => v.Inventory)
                .Where(v => v.ProductId == productId)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}