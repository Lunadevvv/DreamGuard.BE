using System;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        Task<Payment?> GetPaymentByIdAsync(Guid paymentId);
        Task<Payment?> GetPaymentByOrderIdAsync(Guid orderId);
        Task<Payment?> GetPaymentByOrderIdForUpdateAsync(Guid orderId);
        Task<Payment?> GetPaymentByOrderCodeAsync(string orderCode);
        Task<PaginatedList<Payment>> GetPaymentsByUserIdAsync(Guid userId, int pageNumber, PaymentStatus? status);
        Task<PaginatedList<Payment>> GetAllPaymentsForAdminAsync(int pageNumber, PaymentStatus? status, PaymentMethod? method, string? orderCode);
    }
}
