using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;
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
                .Where(v => v.ProductId == productId)
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
    }
}