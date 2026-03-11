using System;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface ICartService
    {
        Task<Result<CartResponse>> GetCartAsync(Guid userId);
        Task<Result> AddToCartAsync(Guid userId, AddToCartRequest request);
        Task<Result> UpdateCartItemAsync(Guid userId, Guid cartItemId, UpdateCartItemRequest request);
        Task<Result> RemoveCartItemAsync(Guid userId, Guid cartItemId);
        Task<Result> ClearCartAsync(Guid userId);
        Task<Result<CartResponse>> SyncCartAsync(Guid userId, SyncCartRequest request);
    }
}
