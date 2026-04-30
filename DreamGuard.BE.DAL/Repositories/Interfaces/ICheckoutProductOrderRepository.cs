using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Models;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface ICheckoutProductOrderRepository : IGenericRepository<CheckoutProductOrder>
    {
        Task<CheckoutProductOrder?> GetWithOrdersByIdAsync(Guid id);
        Task<CheckoutProductOrder?> GetWithOrdersAndPaymentsByIdAsync(Guid id);
        Task<DreamGuard.BE.DAL.ModelExtensions.PaginatedList<CheckoutProductOrder>> GetAllForAdminAsync(int pageNumber, DreamGuard.BE.DAL.Constants.CheckoutOrderStatus? status, string? orderCode);
    }
}
