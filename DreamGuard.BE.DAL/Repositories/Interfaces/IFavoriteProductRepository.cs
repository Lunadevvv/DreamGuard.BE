using System;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface IFavoriteProductRepository : IGenericRepository<FavoriteProduct>
    {
        Task<FavoriteProduct?> GetByCustomerAndProductAsync(Guid customerId, Guid productId);
        Task<FavoriteProduct?> GetByCustomerAndComboAsync(Guid customerId, Guid comboId);
        Task<PaginatedList<FavoriteProduct>> GetFavoritesByCustomerIdAsync(Guid customerId, int pageNumber);
    }
}
