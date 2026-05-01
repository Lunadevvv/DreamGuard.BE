using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Models;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface ICheckoutProductOrderRepository : IGenericRepository<CheckoutProductOrder>
    {
        Task<CheckoutProductOrder?> GetWithOrdersByIdAsync(Guid id);
        Task<CheckoutProductOrder?> GetWithOrdersAndPaymentsByIdAsync(Guid id);
        Task<ModelExtensions.PaginatedList<CheckoutProductOrder>> GetAllForAdminAsync(int pageNumber, Constants.CheckoutOrderStatus? status, string? orderCode);
        Task<ModelExtensions.PaginatedList<CheckoutProductOrder>> GetAllForUserAsync(int pageNumber, Constants.CheckoutOrderStatus? status, string? orderCode, Guid userId);
    }
}
