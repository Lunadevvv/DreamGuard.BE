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
using DreamGuard.BE.DAL.Responses;
using Microsoft.EntityFrameworkCore;

namespace DreamGuard.BE.DAL.Repositories.Implements
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(DreamGuardContext context) : base(context) { }
        public async Task<List<Order>> GetOrderDashBoardAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.Orders.Include(ti => ti.Payments)
                .Where(ti => ti.CreatedAt >= fromDate && ti.CreatedAt < toDate)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(Guid orderId)
        {
            return await _context.Orders
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<Order?> GetOrderWithItemsAsync(Guid orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.VariantCustomizeTypes)
                            .ThenInclude(vct => vct.ProductCustomizeType)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Combo)
                .Include(o => o.UserVoucher)
                    .ThenInclude(uv => uv!.Voucher)
                .Include(o => o.ShippingTasks)
                    .ThenInclude(st => st.Staff)
                .Include(o => o.Payments)
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

        public async Task<PaginatedList<Order>> GetOrdersByCustomerIdAsync(
            Guid customerId, int pageNumber, OrderStatus? status)
        {
            var query = _context.Orders
                .Where(o => o.CustomerId == customerId)
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
        public async Task<List<OrderItem>> GetOrdersToTradeInAsync(Guid customerId, int categoryParentId, decimal salePrice, decimal depositAmount)
        {
            return await _context.OrderItems
                .Include(v => v.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                        .ThenInclude(p => p.Assets)
                .Where(oi => oi.Order!.CustomerId == customerId
                && oi.ProductVariant!.Product!.Category!.CateParentId == categoryParentId
                && oi.TradeInUsedAmount < oi.Quantity && (oi.UnitPrice <= salePrice) && (oi.ProductVariant.Product.MinTradeInPrice <= oi.UnitPrice - depositAmount) 
                && oi.Order.Payments.Any(p => p.PaymentType == PaymentType.Purchase && (p.Status == PaymentStatus.Paid || p.Status == PaymentStatus.CODPaid) ))
                .ToListAsync();
        }
        public async Task<OrderItem?> GetOrderItemByIdAsync(Guid orderItemId)
        {
            return await _context.OrderItems
                .Include(oi => oi.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                        .ThenInclude(p => p.Category)
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.Payments)
                .Include(oi => oi.TradeInOrders)
                .FirstOrDefaultAsync(oi => oi.Id == orderItemId);
        }

        public async Task<List<TopProductSeller>> GetBestSellerProductsAsync(int top)
        {
            var topProducts = await _context.OrderItems
                .Where(oi => oi.Order.Status == OrderStatus.Completed)
                .GroupBy(oi => oi.ProductVariant!.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(top)
                .ToListAsync();
            var topIds = topProducts.Select(tp => tp.ProductId).ToList();
            var products = await _context.Products
                .Where(p => topIds.Contains(p.Id))
                .Include(p => p.Assets)
                .Include(p => p.Variants)
                .ToListAsync();
            var productDict = products.ToDictionary(p => p.Id);
            return topProducts.Select(tp => new TopProductSeller
            {
                Product = productDict[tp.ProductId],
                TotalQuantity = tp.TotalQuantity,
            }).ToList();
        }
    }
}
