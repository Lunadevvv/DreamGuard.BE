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

        public async Task<FavoriteProduct?> GetByUserAndProductAsync(Guid userId, Guid productId)
        {
            return await _context.FavoriteProducts
                .AsTracking()
                .FirstOrDefaultAsync(fp => fp.UserId == userId && fp.ProductId == productId);
        }

        public async Task<PaginatedList<FavoriteProduct>> GetFavoritesByUserIdAsync(Guid userId, int pageNumber)
        {
            var query = _context.FavoriteProducts
                .Where(fp => fp.UserId == userId)
                .Include(fp => fp.Product)
                    .ThenInclude(p => p!.Assets)
                .Include(fp => fp.Product)
                    .ThenInclude(p => p!.Variants)
                .OrderByDescending(fp => fp.CreatedAt)
                .AsSplitQuery()
                .AsNoTracking();

            return await PaginatedList<FavoriteProduct>.CreateAsync(query, pageNumber, 10);
        }
    }
}
