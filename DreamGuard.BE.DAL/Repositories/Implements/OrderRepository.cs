using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.DbContext;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DreamGuard.BE.DAL.Repositories.Implements
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(DreamGuardContext context) : base(context) { }

        public async Task<Order?> GetOrderByIdAsync(Guid orderId)
        {
            return await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<Order?> GetOrderWithItemsAsync(Guid orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Combo)
                .Include(o => o.UserVoucher)
                    .ThenInclude(uv => uv!.Voucher)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<Order?> GetOrderWithItemsForUpdateAsync(Guid orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .AsTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<PaginatedList<Order>> GetOrdersByUserIdAsync(
            Guid userId, int pageNumber, OrderStatus? status)
        {
            var query = _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)
                .AsSplitQuery()
                .AsNoTracking();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            return await PaginatedList<Order>.CreateAsync(query, pageNumber, 10);
        }

        public async Task<PaginatedList<Order>> GetAllOrdersForAdminAsync(
            int pageNumber, OrderStatus? status, string? orderCode)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)
                .AsSplitQuery()
                .AsNoTracking();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            if (!string.IsNullOrEmpty(orderCode))
            {
                query = query.Where(o => o.OrderCode.Contains(orderCode));
            }

            return await PaginatedList<Order>.CreateAsync(query, pageNumber, 10);
        }

        public async Task AddOrderItemsAsync(List<OrderItem> items)
        {
            await _context.OrderItems.AddRangeAsync(items);
            await _context.SaveChangesAsync();
        }
    }
}
