using System;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface IFavoriteProductRepository : IGenericRepository<FavoriteProduct>
    {
        Task<FavoriteProduct?> GetByUserAndProductAsync(Guid userId, Guid productId);
        Task<PaginatedList<FavoriteProduct>> GetFavoritesByUserIdAsync(Guid userId, int pageNumber);
    }
}
