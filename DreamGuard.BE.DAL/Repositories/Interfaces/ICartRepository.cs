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
        Task<int> AddCartItemAsync(CartItem item);
        Task<int> UpdateCartItemAsync(CartItem item);
        Task<bool> RemoveCartItemAsync(CartItem item);
        Task RemoveAllCartItemsAsync(Guid cartId);
    }
}
