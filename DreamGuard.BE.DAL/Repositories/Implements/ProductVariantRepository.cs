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

        public async Task<List<ProductVariant>> GetVariantsByProductIdAsync(Guid productId)
        {
            return await _context.ProductVariants
                .Where(v => v.ProductId == productId)
                .OrderByDescending(v => v.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ProductVariant?> GetVariantByIdAsync(Guid id)
        {
            return await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == id);
        }
    }
}