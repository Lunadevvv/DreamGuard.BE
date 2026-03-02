using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.DbContext;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DreamGuard.BE.DAL.Repositories.Implements
{
    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        public CartRepository(DreamGuardContext context) : base(context) { }

        public async Task<Cart?> GetCartByUserIdAsync(Guid userId)
        {
            return await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Cart?> GetCartWithItemsAsync(Guid userId)
        {
            return await _context.Carts
                .Include(c => c.CartItems.OrderByDescending(ci => ci.CreatedAt))
                    .ThenInclude(ci => ci.ProductVariant!)
                        .ThenInclude(pv => pv.Product!)
                            .ThenInclude(p => p.Assets)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant!)
                        .ThenInclude(pv => pv.Inventory)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Combo)
                .AsSplitQuery()
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<CartItem?> GetCartItemAsync(Guid cartId, Guid? productVariantId, Guid? comboId)
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(ci =>
                    ci.CartId == cartId &&
                    ci.ProductVariantId == productVariantId &&
                    ci.ComboId == comboId);
        }

        public async Task<CartItem?> GetCartItemByIdAsync(Guid cartItemId)
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);
        }

        public async Task<int> AddCartItemAsync(CartItem item)
        {
            _context.CartItems.Add(item);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateCartItemAsync(CartItem item)
        {
            _context.CartItems.Update(item);
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> RemoveCartItemAsync(CartItem item)
        {
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task RemoveAllCartItemsAsync(Guid cartId)
        {
            var items = await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .ToListAsync();
            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
    }
}
