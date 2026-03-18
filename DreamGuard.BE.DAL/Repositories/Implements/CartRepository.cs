using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<Cart?> GetCartByCustomerIdAsync(Guid customerId)
        {
            return await _context.Carts
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

        public async Task<Cart?> GetCartWithItemsAsync(Guid customerId)
        {
            return await _context.Carts
                .Include(c => c.CartItems.OrderByDescending(ci => ci.AddedAt))
                    .ThenInclude(ci => ci.ProductVariant!)
                        .ThenInclude(pv => pv.Product)
                            .ThenInclude(p => p!.Assets)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant!)
                        .ThenInclude(pv => pv.Inventory)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Combo!)
                        .ThenInclude(combo => combo.ComboParent)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Combo!)
                        .ThenInclude(combo => combo.ComboProductVariants)
                            .ThenInclude(cpv => cpv.ProductVariant!)
                                .ThenInclude(pv => pv.Inventory)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

        public async Task<CartItem?> GetCartItemAsync(Guid cartId, Guid? productVariantId, Guid? comboId)
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cartId
                    && ci.ProductVariantId == productVariantId
                    && ci.ComboId == comboId);
        }

        public async Task<CartItem?> GetCartItemByIdAsync(Guid cartItemId)
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);
        }

        public async Task AddCartItemAsync(CartItem cartItem)
        {
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCartItemAsync(CartItem cartItem)
        {
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveCartItemAsync(CartItem cartItem)
        {
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
        }

        public async Task ClearCartItemsAsync(Guid cartId)
        {
            var items = await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .ToListAsync();
            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CartItem>> GetCartItemsByCartIdAsync(Guid cartId)
        {
            return await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .ToListAsync();
        }
    }
}
