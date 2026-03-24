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
    public class VariantCustomizeTypeRepository : GenericRepository<VariantCustomizeType>, IVariantCustomizeTypeRepository
    {
        private readonly DreamGuardContext _context;

        public VariantCustomizeTypeRepository(DreamGuardContext context) : base(context)
        {
            _context = context;
        }

        public async Task<VariantCustomizeType?> GetByCompositeKeyAsync(Guid cusId, Guid productVariantId)
        {
            return await _context.VariantCustomizeTypes
                .FirstOrDefaultAsync(vct => vct.CusId == cusId && vct.ProductVariantId == productVariantId);
        }

        public async Task<List<VariantCustomizeType>> GetByVariantIdAsync(Guid productVariantId)
        {
            return await _context.VariantCustomizeTypes
                .Where(vct => vct.ProductVariantId == productVariantId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<VariantCustomizeType>> GetByVariantIdWithDetailsAsync(Guid productVariantId)
        {
            return await _context.VariantCustomizeTypes
                .Include(vct => vct.ProductCustomizeType)
                .Where(vct => vct.ProductVariantId == productVariantId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<VariantCustomizeType> entities)
        {
            await _context.VariantCustomizeTypes.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }
    }
}
