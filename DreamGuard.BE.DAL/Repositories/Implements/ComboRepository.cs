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
    public class ComboRepository : GenericRepository<Combo>, IComboRepository
    {
        public ComboRepository(DreamGuardContext context) : base(context)
        {
        }

        public async Task<PaginatedList<Combo>> GetAllCombosAsync(
            int pageNumber, double? maxPrice, int? maxAgeGroup, string? color)
        {
            var query = _context.Combos
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.AverageRating)
                .AsNoTracking();

            if (maxPrice.HasValue)
            {
                query = query.Where(c => c.SalePrice <= maxPrice.Value);
            }

            if (maxAgeGroup.HasValue)
            {
                query = query.Where(c => c.AgeGroup <= maxAgeGroup.Value);
            }

            if (!string.IsNullOrEmpty(color))
            {
                query = query.Where(c =>
                    c.ComboProductVariants.Any(cpv =>
                        cpv.ProductVariant != null &&
                        cpv.ProductVariant.Attributes != null &&
                        cpv.ProductVariant.Attributes.Color == color));
            }

            return await PaginatedList<Combo>.CreateAsync(query, pageNumber, 10);
        }

        public async Task<Combo?> GetComboByIdAsync(Guid id)
        {
            return await _context.Combos
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        }

        public async Task<Combo?> GetComboWithChildrenAsync(Guid id)
        {
            return await _context.Combos
                .Include(c => c.ComboChildrens.Where(ch => ch.IsActive))
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        }

        public async Task<Combo?> GetComboWithProductsAsync(Guid id)
        {
            return await _context.Combos
                .Include(c => c.ComboProductVariants)
                    .ThenInclude(cpv => cpv.ProductVariant!)
                        .ThenInclude(pv => pv.Product)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        }

        public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null)
        {
            var query = _context.Combos.Where(c => c.Slug == slug && c.IsActive);
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }

        public async Task RemoveComboProductVariantsAsync(Guid comboId)
        {
            var items = await _context.ComboProductVariants
                .Where(cpv => cpv.ComboId == comboId)
                .ToListAsync();
            _context.ComboProductVariants.RemoveRange(items);
            await _context.SaveChangesAsync();
        }

        public async Task AddComboProductVariantsAsync(List<ComboProductVariant> items)
        {
            await _context.ComboProductVariants.AddRangeAsync(items);
            await _context.SaveChangesAsync();
        }
    }
}
