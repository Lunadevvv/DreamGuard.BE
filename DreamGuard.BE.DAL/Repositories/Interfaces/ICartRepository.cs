using System;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<Cart?> GetCartByUserIdAsync(Guid userId);
        Task<Cart?> GetCartWithItemsAsync(Guid userId);
        Task<CartItem?> GetCartItemAsync(Guid cartId, Guid? productVariantId, Guid? comboId);
        Task<CartItem?> GetCartItemByIdAsync(Guid cartItemId);
        Task AddCartItemAsync(CartItem cartItem);
        Task UpdateCartItemAsync(CartItem cartItem);
        Task RemoveCartItemAsync(CartItem cartItem);
        Task ClearCartItemsAsync(Guid cartId);
    }
}
