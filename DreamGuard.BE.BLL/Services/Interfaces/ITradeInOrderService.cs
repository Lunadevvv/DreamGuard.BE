using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface ITradeInOrderService
    {
        Task<Result> CancelAsync(Guid tradeInOrderId);
        Task<Result> ConfirmAsync(Guid tradeInOrderId, decimal tradeInPrice);
        Task<Result> ProcessingAsync(Guid tradeInOrderId);
        Task<Result> DeliveredAsync(Guid tradeInOrderId);
        Task<Result> CompletedAsync(Guid tradeInOrderId);
        Task<Result<CreateTradeInOrderResponse>> TradeInOrderAsync(CreateTradeInOrderRequest request, Guid customerId, string ipAddress);
        Task<Result<CreateTradeInOrderResponse>> ReOrderTradeInAsync(Guid tradeInOrderId, Guid customerId, string ipAddress);
        Task<Result> UploadTradeInOrderImageAsync(Guid tradeInOrderId, TradeInOrderImageCreateRequest request);
        Task<Result<CalculateTradeInOrderPriceResponse>> CalculatePriceAsync(CalculateTradeInOrderPriceRequest request);
        Task<Result<PaginatedList<TradeInOrderSummaryResponse>>> GetMyOrdersAsync(Guid customerId, int pageNumber, int pageSize);
        Task<Result<PaginatedList<TradeInOrderSummaryResponse>>> GetWaitingOrdersAsync(int pageNumber, int pageSize);
        Task<Result<TradeInOrderDetailResponse>> GetOrderDetailByIdAsync(Guid tradeInOrderId);
        Task<Result<PaginatedList<TradeInOrderSummaryResponse>>> AdminSearchTradeInOrder(Guid? customerId, Guid? productVariantId, TradeInOrderStatus? status, bool? isGood, decimal? tradeInPrice, decimal? amountToPay, decimal? depositAmount, string? phoneNumber, int pageNumber, int pageSize);
        Task<Result> CreateConversationAsync(Guid tradeInOrderId, Guid staffId);
    }
}
