using System;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.DAL.ModelExtensions;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IFavoriteProductService
    {
        Task<Result> AddToFavoriteAsync(Guid userId, Guid productId);
        Task<Result> RemoveFromFavoriteAsync(Guid userId, Guid productId);
        Task<Result<PaginatedList<FavoriteProductResponse>>> GetFavoriteProductsAsync(Guid userId, int pageNumber);
    }
}
