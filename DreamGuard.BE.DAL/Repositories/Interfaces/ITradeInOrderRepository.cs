using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface ITradeInOrderRepository : IGenericRepository<TradeInOrder>
    {
        Task<List<TradeInOrder>> GetTradeInOrderDashBoardAsync(DateTime fromDate, DateTime toDate);
        Task<TradeInOrder?> GetTradeInByIdAsync(Guid tradeInOrderId);
        Task<PaginatedList<TradeInOrder>> GetMyOrdersAsync(Guid customerId, int pageNumber, int pageSize);
        Task<PaginatedList<TradeInOrder>> GetWaitingOrdersAsync(int pageNumber, int pageSize);
        Task<PaginatedList<TradeInOrder>> AdminSearchOrderAsync(Guid? customerId, Guid? productVariantId, TradeInOrderStatus? status, bool? isGood, decimal? tradeInPrice, decimal? amountToPay, decimal? depositAmount, string? phoneNumber, int pageNumber, int pageSize);
        Task<TradeInOrder?> GetOrderDetailById(Guid tradeInOrderId);
        Task<int> UpdateStatusIfMatch(Guid tradeInOrderId, TradeInOrderStatus newStatus, List<TradeInOrderStatus> allowedStatuses);
    }
}
