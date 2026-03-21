using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.DbContext;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;

namespace DreamGuard.BE.DAL.Repositories.Implements
{
    public class ProductCustomizeTypeRepository : GenericRepository<ProductCustomizeType>, IProductCustomizeTypeRepository
    {
        private readonly DreamGuardContext _context;
        public ProductCustomizeTypeRepository(DreamGuardContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PaginatedList<ProductCustomizeType>> GetAllWithPagingAsync(int pageNumber, int pageSize)
        {
            var query = _context.ProductCustomizeTypes;
            return await PaginatedList<ProductCustomizeType>.CreateAsync(query, pageNumber, pageSize);
        }
    }
}