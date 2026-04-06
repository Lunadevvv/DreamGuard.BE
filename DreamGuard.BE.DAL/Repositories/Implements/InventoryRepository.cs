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
    public class InventoryRepository : GenericRepository<Inventory>, IInventoryRepository
    {
        public InventoryRepository(DreamGuardContext context) : base(context)
        {
        }

        public async Task<Inventory?> GetInventoryByVariantIdAsync(Guid productVariantId)
        {
            return await _context.Inventories.FirstOrDefaultAsync(i => i.ProductVariantId == productVariantId);
        }

        public async Task<Inventory?> GetInventoryByVariantIdForUpdateAsync(Guid productVariantId)
        {
            return await _context.Inventories
                .Include(i => i.ProductVariant)
                .AsTracking()
                .FirstOrDefaultAsync(i => i.ProductVariantId == productVariantId);
        }

        public async Task<List<Inventory>> GetInventoriesByVariantIdsAsync(List<Guid> variantIds)
        {
            return await _context.Inventories
                .Where(i => variantIds.Contains(i.ProductVariantId))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Inventory>> GetInventoriesByVariantIdsForUpdateAsync(List<Guid> variantIds)
        {
            return await _context.Inventories
                .Include(i => i.ProductVariant)
                .Where(i => variantIds.Contains(i.ProductVariantId))
                .AsTracking()
                .ToListAsync();
        }

        public async Task<int> ReduceInventoryStock(Guid productVariantId)
        {
            var result = await _context.Inventories
                .Where(iv => iv.ProductVariantId == productVariantId && iv.Quantity > 0)
                .ExecuteUpdateAsync(s => 
                s
                .SetProperty(iv => iv.Quantity, iv => iv.Quantity - 1)
                .SetProperty(iv => iv.UpdatedAt, iv => DateTime.UtcNow)
                );
            if (result > 0)
            {
                // Kiểm tra xem sau khi trừ có về 0 không để cập nhật Status của Variant
                await _context.ProductVariants
                    .Where(pv => pv.Id == productVariantId && _context.Inventories.Any(iv => iv.ProductVariantId == pv.Id && iv.Quantity == 0))
                    .ExecuteUpdateAsync(s => s.SetProperty(pv => pv.Status, ProductStatus.OutOfStock));
            }
            return result;
        }
        public async Task<int> IncreaseInventoryStock(Guid productVariantId)
        {
            var result = await _context.Inventories
                .Where(iv => iv.ProductVariantId == productVariantId)
                .ExecuteUpdateAsync(s =>
                s
                .SetProperty(iv => iv.Quantity, iv => iv.Quantity + 1)
                .SetProperty(iv => iv.UpdatedAt, iv => DateTime.UtcNow)
                );
            if (result > 0)
            {
                // Nếu hàng tăng lên > 0, phải mở lại Status cho khách mua
                // Chỉ update nếu Status hiện tại đang là OutOfStock
                await _context.ProductVariants
                    .Where(pv => pv.Id == productVariantId
                                 && pv.Status == ProductStatus.OutOfStock
                                 && _context.Inventories.Any(iv => iv.ProductVariantId == pv.Id && iv.Quantity > 0))
                    .ExecuteUpdateAsync(s => s.SetProperty(pv => pv.Status, ProductStatus.Published));
            }
            return result;
        }
    }
}