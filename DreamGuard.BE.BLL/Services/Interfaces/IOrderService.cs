using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Result<OrderResponse>> CheckoutAsync(Guid userId, CheckoutRequest request);
        Task<Result<OrderResponse>> GetOrderByIdAsync(Guid userId, Guid orderId);
        Task<Result<PaginatedList<OrderListResponse>>> GetMyOrdersAsync(Guid userId, int pageNumber, OrderStatus? status);
        Task<Result<PaginatedList<OrderListResponse>>> GetAllOrdersAsync(int pageNumber, OrderStatus? status);
        Task<Result<OrderResponse>> GetOrderByIdForAdminAsync(Guid orderId);
        Task<Result> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request);
        Task<Result> CancelOrderAsync(Guid userId, Guid orderId);
    }
}
