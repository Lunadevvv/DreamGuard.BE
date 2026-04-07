using System;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<Order?> GetOrderByIdAsync(Guid orderId);
        Task<Order?> GetOrderWithItemsAsync(Guid orderId);
        Task<Order?> GetOrderWithItemsForUpdateAsync(Guid orderId);
        Task<PaginatedList<Order>> GetOrdersByCustomerIdAsync(Guid customerId, int pageNumber, OrderStatus? status);
        Task<PaginatedList<Order>> GetAllOrdersForAdminAsync(int pageNumber, OrderStatus? status, string? orderCode);
        Task AddOrderItemsAsync(List<OrderItem> items);
        Task<OrderItem?> GetOrderItemByIdAsync(Guid orderItemId);
    }
}
