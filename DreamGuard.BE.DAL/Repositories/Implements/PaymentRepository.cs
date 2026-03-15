using System;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.DbContext;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DreamGuard.BE.DAL.Repositories.Implements
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(DreamGuardContext context) : base(context) { }

        public async Task<Payment?> GetPaymentByIdAsync(Guid paymentId)
        {
            return await _context.Payments
                .Include(p => p.POrder)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == paymentId);
        }

        public async Task<Payment?> GetPaymentByOrderIdAsync(Guid orderId)
        {
            return await _context.Payments
                .Include(p => p.POrder)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.POrderId == orderId);
        }

        public async Task<Payment?> GetPaymentByOrderIdForUpdateAsync(Guid orderId)
        {
            return await _context.Payments
                .Include(p => p.POrder)
                .AsTracking()
                .FirstOrDefaultAsync(p => p.POrderId == orderId);
        }

        public async Task<Payment?> GetPaymentByOrderCodeAsync(string orderCode)
        {
            return await _context.Payments
                .Include(p => p.POrder)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.OrderCode == orderCode);
        }

        public async Task<PaginatedList<Payment>> GetPaymentsByUserIdAsync(
            Guid userId, int pageNumber, PaymentStatus? status)
        {
            var query = _context.Payments
                .Include(p => p.POrder)
                .Where(p => p.POrder != null && p.POrder.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking();

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            return await PaginatedList<Payment>.CreateAsync(query, pageNumber, 10);
        }

        public async Task<PaginatedList<Payment>> GetAllPaymentsForAdminAsync(
            int pageNumber, PaymentStatus? status, PaymentMethod? method, string? orderCode)
        {
            var query = _context.Payments
                .Include(p => p.POrder)
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking();

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            if (method.HasValue)
            {
                query = query.Where(p => p.PaymentMethod == method.Value);
            }

            if (!string.IsNullOrEmpty(orderCode))
            {
                query = query.Where(p => p.OrderCode.Contains(orderCode));
            }

            return await PaginatedList<Payment>.CreateAsync(query, pageNumber, 10);
        }


    }
}
