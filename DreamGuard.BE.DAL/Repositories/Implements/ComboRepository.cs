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
    public class ComboRepository : GenericRepository<Combo>, IComboRepository
    {
        public ComboRepository(DreamGuardContext context) : base(context)
        {
        }

        public async Task<PaginatedList<Combo>> GetAllCombosAsync(
            int pageNumber, decimal? maxPrice, int? maxAgeGroup, string? color)
        {
            // Only with active parent combos
            var query = _context.Combos
                .Where(c => c.Status == ProductStatus.Published && c.ComboParentId == null)
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
                    c.Color == color);
            }

            return await PaginatedList<Combo>.CreateAsync(query, pageNumber, 10);
        }

        public async Task<Combo?> GetComboByIdAsync(Guid id)
        {
            return await _context.Combos
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Combo?> GetComboWithChildrenAsync(Guid id, string? size, string? color)
        {
            var query = await _context.Combos
                .Include(c => c.ComboChildrens.Where(ch => ch.Status == ProductStatus.Published))
                    .ThenInclude(ch => ch.ComboProductVariants)
                        .ThenInclude(cpv => cpv.ProductVariant!)
                            .ThenInclude(pv => pv.Inventory)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (query != null && query.ComboChildrens.Any()){
                if (!string.IsNullOrEmpty(size))
                {
                    query.ComboChildrens = query.ComboChildrens.Where(c => c.Size == size).ToList();
                }
                if (!string.IsNullOrEmpty(color))
                {
                    query.ComboChildrens = query.ComboChildrens.Where(c => c.Color == color).ToList();
                }
            }

            return query;
        }

        public async Task<Combo?> GetComboWithProductsAsync(Guid id)
        {
            return await _context.Combos
                .Include(c => c.ComboProductVariants)
                    .ThenInclude(cpv => cpv.ProductVariant!)
                        .ThenInclude(pv => pv.Product)
                .Include(c => c.ComboProductVariants)
                    .ThenInclude(cpv => cpv.ProductVariant!)
                        .ThenInclude(pv => pv.Inventory)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null)
        {
            var query = _context.Combos.Where(c => c.Slug == slug);
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

        public async Task<Combo?> GetComboByIdForUpdateAsync(Guid id)
        {
            return await _context.Combos
                .AsTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Combo?> GetComboWithProductsForUpdateAsync(Guid id)
        {
            return await _context.Combos
                .Include(c => c.ComboProductVariants)
                    .ThenInclude(cpv => cpv.ProductVariant!)
                        .ThenInclude(pv => pv.Inventory)
                .AsSplitQuery()
                .AsTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Combo>> GetAllChildrenOfParentAsync(Guid parentId)
        {
            return await _context.Combos
                .Where(c => c.ComboParentId == parentId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PaginatedList<Combo>> GetAllCombosForAdminAsync(int pageNumber, string? name, ProductStatus? status)
        {
            var query = _context.Combos
                .Include(c => c.ComboChildrens)
                .OrderByDescending(c => c.CreatedAt)
                .AsNoTracking();

            if(status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(c =>
                    c.Name == name);
            }

            return await PaginatedList<Combo>.CreateAsync(query, pageNumber, 10);
        }

        public async Task<List<Combo>> GetOutOfStockCombosByVariantIdAsync(Guid productVariantId)
        {
            var comboIds = await _context.ComboProductVariants
                .Where(cpv => cpv.ProductVariantId == productVariantId)
                .Select(cpv => cpv.ComboId)
                .Distinct()
                .ToListAsync();

            if (!comboIds.Any())
                return new List<Combo>();

            return await _context.Combos
                .Where(c => comboIds.Contains(c.Id)
                            && c.Status == ProductStatus.OutOfStock
                            && c.ComboParentId != null)
                .Include(c => c.ComboProductVariants)
                    .ThenInclude(cpv => cpv.ProductVariant!)
                        .ThenInclude(pv => pv.Inventory)
                .AsSplitQuery()
                .AsTracking()
                .ToListAsync();
        }

    }
}
