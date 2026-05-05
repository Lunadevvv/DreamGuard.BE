using System;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Responses;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<List<Order>> GetOrderDashBoardAsync(DateTime fromDate, DateTime toDate);
        Task<Order?> GetOrderByIdAsync(Guid orderId);
        Task<Order?> GetOrderWithItemsAsync(Guid orderId);
        Task<Order?> GetOrderWithItemsForUpdateAsync(Guid orderId);
        Task<PaginatedList<Order>> GetOrdersByCustomerIdAsync(Guid customerId, int pageNumber, OrderStatus? status);
        Task<PaginatedList<Order>> GetAllOrdersForAdminAsync(int pageNumber, OrderStatus? status, string? orderCode);
        void AddOrderItems(List<OrderItem> items);
        Task<List<OrderItem>> GetOrdersToTradeInAsync(Guid customerId, int categoryParentId, decimal salePrice, decimal depositAmount);
        Task<OrderItem?> GetOrderItemByIdAsync(Guid orderItemId);
        Task<List<TopProductSeller>> GetBestSellerProductsAsync(int top);
        Task<List<Order>> GetOrdersByCheckoutOrderIdAsync(Guid checkoutProductOrderId);


    }
}
