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
        Task<PaginatedList<Order>> GetOrdersByUserIdAsync(Guid userId, int pageNumber, OrderStatus? status);
        Task<PaginatedList<Order>> GetAllOrdersAsync(int pageNumber, OrderStatus? status);
        Task<int> AddOrderItemsAsync(List<OrderItem> items);
    }
}
