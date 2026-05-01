using System;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Result<List<TotalAmountLineChartResponse>>> GetTotalAmountLineChartAsync(DateOnly fromDate, DateOnly toDate);
        Task<Result<OrderDashBoardResponse>> GetOrderDashBoardAsync(DateOnly fromDate, DateOnly toDate);
        Task<Result<CheckoutProductOrderResponse>> CreateOrderAsync(Guid userId, CreateOrderRequest request, string ipAddress);
        Task<Result<OrderResponse>> CreateOrderByAdminAsync(Guid adminId, CreateOrderByAdminRequest request, string ipAddress);
        Task<Result<OrderDetailResponse>> GetOrderByIdAsync(Guid orderId);
        Task<Result<PaginatedList<OrderSummaryResponse>>> GetOrdersAsync(Guid userId, int pageNumber, OrderStatus? status);
        Task<Result<PaginatedList<OrderSummaryResponse>>> GetAllOrdersForAdminAsync(int pageNumber, OrderStatus? status, string? orderCode);
        Task<Result<PaginatedList<CheckoutProductOrderAdminSummaryResponse>>> GetAllCheckoutOrdersForAdminAsync(int pageNumber, CheckoutOrderStatus? status, string? orderCode);
        Task<Result<PaginatedList<CheckoutProductOrderAdminSummaryResponse>>> GetAllUserCheckoutOrdersAsync(int pageNumber, CheckoutOrderStatus? status, string? orderCode, Guid userId);
        Task<Result> UpdateCheckoutOrderStatusAsync(Guid checkoutOrderId, CheckoutOrderStatus newStatus);
        Task<Result> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus);
        Task<Result> CancelOrderAsync(Guid userId, Guid orderId);
        Task<Result<List<OrderItemResponse>>> GetOrdersToTradeInAsync(Guid customerId, Guid productVariantId);
    }
}
