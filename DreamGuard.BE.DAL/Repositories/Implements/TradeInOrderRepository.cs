using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.DbContext;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Repositories.Implements
{
    public class TradeInOrderRepository : GenericRepository<TradeInOrder>, ITradeInOrderRepository
    {
        public TradeInOrderRepository(DreamGuardContext context) : base(context)
        {
        }
        public async Task<TradeInOrder?> GetOrderDetailById(Guid tradeInOrderId)
        {
            return await _context.TradeInOrders
                .Include(ti => ti.OrderItem)
                .Include(ti => ti.ProductVariant)
                    .ThenInclude(pv => pv.Inventory)
                .Include(ti => ti.TradeInImages)
                .Include(ti => ti.Payments)
                .Include(ti => ti.Conversation)
                .Where(o => o.TradeInOrderId == tradeInOrderId).FirstOrDefaultAsync();
        }
        public async Task<TradeInOrder?> GetTradeInByIdAsync(Guid tradeInOrderId)
        {
            return await _context.TradeInOrders
                .Include(ti => ti.Payments)
                .Include(ti => ti.ProductVariant)
                    .ThenInclude(pv => pv.Inventory)
                .Include(ti => ti.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .Include(ti => ti.OrderItem)
                .Where(o => o.TradeInOrderId == tradeInOrderId).FirstOrDefaultAsync();
        }

        public async Task<PaginatedList<TradeInOrder>> GetMyOrdersAsync(Guid customerId, int pageNumber, int pageSize)
        {
            var query = _context.TradeInOrders
                .Where(o => o.CustomerId == customerId).OrderByDescending(o => o.CreatedAt);
            return await PaginatedList<TradeInOrder>.CreateAsync(query, pageNumber, pageSize);
        }
        public async Task<PaginatedList<TradeInOrder>> AdminSearchOrderAsync(Guid? customerId, Guid? productVariantId, TradeInOrderStatus? status, bool? isGood, decimal? tradeInPrice, decimal? amountToPay, decimal? depositAmount, string? phoneNumber, int pageNumber, int pageSize)
        {
            var query = _context.TradeInOrders
                .Where(o => (customerId == null || o.CustomerId == customerId)
                && (productVariantId == null || o.ProductVariantId == productVariantId)
                && (status == null || o.Status == status)
                && (isGood == null || o.IsGood == isGood)
                && (tradeInPrice == null || o.TradeInPrice >= tradeInPrice)
                && (amountToPay == null || o.AmountToPay >= amountToPay)
                && (depositAmount == null || o.DepositAmount >= depositAmount)
                && (string.IsNullOrEmpty(phoneNumber) || o.PhoneNumber.Contains(phoneNumber))
                )
                .OrderByDescending(o => o.CreatedAt);
            return await PaginatedList<TradeInOrder>.CreateAsync(query, pageNumber, pageSize);
        }
        public async Task<int> UpdateStatusIfMatch(Guid tradeInOrderId, TradeInOrderStatus newStatus, List<TradeInOrderStatus> allowedStatuses)
        {
            return await _context.TradeInOrders
                .Where(x => x.TradeInOrderId == tradeInOrderId
                         && allowedStatuses.Contains(x.Status))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, x => newStatus)
                );
        }

        public async Task<PaginatedList<TradeInOrder>> GetWaitingOrdersAsync(int pageNumber, int pageSize)
        {
            var query = _context.TradeInOrders
                .Where(ti => ti.Status == TradeInOrderStatus.WAITING_FOR_STAFF).OrderByDescending(o => o.CreatedAt);
            return await PaginatedList<TradeInOrder>.CreateAsync(query, pageNumber, pageSize);
        }
    }
}
