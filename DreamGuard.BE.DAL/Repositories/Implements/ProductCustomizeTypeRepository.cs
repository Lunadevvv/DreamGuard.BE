using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.DbContext;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using DreamGuard.BE.DAL.Constants;
using Microsoft.EntityFrameworkCore;

namespace DreamGuard.BE.DAL.Repositories.Implements
{
    public class ProductCustomizeTypeRepository : GenericRepository<ProductCustomizeType>, IProductCustomizeTypeRepository
    {
        private readonly DreamGuardContext _context;
        public ProductCustomizeTypeRepository(DreamGuardContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Guid>> GetAllCustomizeTypeIds()
        {
            var ids = await _context.ProductCustomizeTypes
                .Select(pct => pct.Id)
                .ToListAsync() ?? new List<Guid>();

            return ids;
        }

        public async Task<List<Guid>> GetCustomizeTypeIdsByProductTypeAsync(FullyCustomizedProductType type)
        {
            var ids = await _context.ProductCustomizeTypes
                .Where(pct => pct.ApplicableProductType == FullyCustomizedProductType.None || pct.ApplicableProductType == type)
                .Select(pct => pct.Id)
                .ToListAsync() ?? new List<Guid>();

            return ids;
        }

        public async Task<PaginatedList<ProductCustomizeType>> GetAllWithPagingAsync(int pageNumber, int pageSize, List<Guid> exceedProductCustomizeIds)
        {
            var query = _context.ProductCustomizeTypes
                .Where(pct => !exceedProductCustomizeIds.Contains(pct.Id));
                
            return await PaginatedList<ProductCustomizeType>.CreateAsync(query, pageNumber, pageSize);
        }

        public async Task<List<ProductCustomizeType>> GetByIdsAsync(List<Guid> ids)
        {
            return await _context.ProductCustomizeTypes
                .Where(pct => ids.Contains(pct.Id))
                .AsNoTracking()
                .ToListAsync();
        }
    }
}