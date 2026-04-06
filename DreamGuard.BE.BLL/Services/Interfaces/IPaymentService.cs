using System;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<Result<CreatePaymentResponse>> CreatePaymentAsync(Guid orderId, PaymentMethod method, string ipAddress);
        Task<Result<VnPaymentResponse>> HandleVnPayCallbackAsync(Microsoft.AspNetCore.Http.IQueryCollection queryParams);
        Task<Result<PaymentResponse>> GetPaymentByIdAsync(Guid userId, Guid paymentId);
        Task<Result<PaymentResponse>> GetPaymentByOrderIdAsync(Guid userId, Guid orderId);
        Task<Result<PaginatedList<PaymentSummaryResponse>>> GetPaymentsByUserAsync(Guid userId, int pageNumber, PaymentStatus? status);
        Task<Result<PaginatedList<PaymentSummaryResponse>>> GetAllPaymentsForAdminAsync(int pageNumber, PaymentStatus? status, PaymentMethod? method, string? orderCode);
        Task<Result<PaymentResponse>> GetPaymentDetailForAdminAsync(Guid paymentId);
        Task<Result> UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus newStatus);
        Task<Result> ExpirePayment(Guid paymentId);
    }
}
