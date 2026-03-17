using System;
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
    public class FavoriteProductRepository : GenericRepository<FavoriteProduct>, IFavoriteProductRepository
    {
        public FavoriteProductRepository(DreamGuardContext context) : base(context) { }

        public async Task<FavoriteProduct?> GetByCustomerAndProductAsync(Guid customerId, Guid productId)
        {
            return await _context.FavoriteProducts
                .AsTracking()
                .FirstOrDefaultAsync(fp => fp.CustomerId == customerId && fp.ProductId == productId);
        }

        public async Task<FavoriteProduct?> GetByCustomerAndComboAsync(Guid customerId, Guid comboId)
        {
            return await _context.FavoriteProducts
                .AsTracking()
                .FirstOrDefaultAsync(fp => fp.CustomerId == customerId && fp.ComboId == comboId);
        }

        public async Task<PaginatedList<FavoriteProduct>> GetFavoritesByCustomerIdAsync(Guid customerId, int pageNumber)
        {
            var query = _context.FavoriteProducts
                .Where(fp => fp.CustomerId == customerId)
                .Include(fp => fp.Product)
                    .ThenInclude(p => p!.Assets)
                .Include(fp => fp.Product)
                    .ThenInclude(p => p!.Variants)
                .Include(fp => fp.Combo)
                .OrderByDescending(fp => fp.CreatedAt)
                .AsSplitQuery()
                .AsNoTracking();

            return await PaginatedList<FavoriteProduct>.CreateAsync(query, pageNumber, 10);
        }
    }
}
