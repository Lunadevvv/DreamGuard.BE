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
    public class InventoryRepository : GenericRepository<Inventory>, IInventoryRepository
    {
        public InventoryRepository(DreamGuardContext context) : base(context)
        {
        }

        public async Task<Inventory?> GetInventoryByVariantIdAsync(Guid productVariantId)
        {
            return await _context.Inventories.FirstOrDefaultAsync(i => i.ProductVariantId == productVariantId);
        }
    }
}