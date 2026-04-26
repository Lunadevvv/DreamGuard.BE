using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.DbContext;
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
    public class OrderItemRepository : GenericRepository<OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(DreamGuardContext context) : base(context)
        {
        }
        public async Task<bool> IncreaseTradeInUsedAmountAsync(Guid orderItemId)
        {
           var result = await _context.OrderItems
                .Where(oi => oi.Id == orderItemId && oi.TradeInUsedAmount + 1 <= oi.Quantity)
                .ExecuteUpdateAsync(oi => oi.SetProperty(o => o.TradeInUsedAmount, o => o.TradeInUsedAmount + 1));
           if (result == 0)
           {
               return false;
           }
           return true;
        }
        public async Task<bool> DecreaseTradeInUsedAmountAsync(Guid orderItemId)
        {
            var result = await _context.OrderItems
                 .Where(oi => oi.Id == orderItemId && oi.TradeInUsedAmount - 1 >= 0)
                 .ExecuteUpdateAsync(oi => oi.SetProperty(o => o.TradeInUsedAmount, o => o.TradeInUsedAmount - 1));
            if (result == 0)
            {
                return false;
            }
            return true;
        }
    }
}
