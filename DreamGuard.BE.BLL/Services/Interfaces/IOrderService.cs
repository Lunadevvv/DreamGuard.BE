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
        Task<Result<OrderResponse>> CreateOrderAsync(Guid userId, CreateOrderRequest request);
        Task<Result<OrderDetailResponse>> GetOrderByIdAsync(Guid userId, Guid orderId);
        Task<Result<PaginatedList<OrderSummaryResponse>>> GetOrdersAsync(Guid userId, int pageNumber, OrderStatus? status);
        Task<Result<PaginatedList<OrderSummaryResponse>>> GetAllOrdersForAdminAsync(int pageNumber, OrderStatus? status, string? orderCode);
        Task<Result> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus);
        Task<Result> CancelOrderAsync(Guid userId, Guid orderId);
    }
}
