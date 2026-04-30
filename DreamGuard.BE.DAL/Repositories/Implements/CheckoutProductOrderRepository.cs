using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.DbContext;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Repositories.Implements
{
    public class CheckoutProductOrderRepository : GenericRepository<CheckoutProductOrder>, ICheckoutProductOrderRepository
    {
        public CheckoutProductOrderRepository(DreamGuardContext context) : base(context)
        {
        }

        public async Task<CheckoutProductOrder?> GetWithOrdersByIdAsync(Guid id)
        {
            return await _context.CheckoutProductOrders
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<CheckoutProductOrder?> GetWithOrdersAndPaymentsByIdAsync(Guid id)
        {
            return await _context.CheckoutProductOrders
                .Include(c => c.Orders)
                .Include(c => c.Payments)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<DreamGuard.BE.DAL.ModelExtensions.PaginatedList<CheckoutProductOrder>> GetAllForAdminAsync(int pageNumber, DreamGuard.BE.DAL.Constants.CheckoutOrderStatus? status, string? orderCode)
        {
            var query = _context.CheckoutProductOrders
                .Include(c => c.Orders)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            if (!string.IsNullOrEmpty(orderCode))
            {
                query = query.Where(o => o.CheckoutOrderCode.Contains(orderCode));
            }

            query = query.OrderByDescending(o => o.CreatedAt);

            int pageSize = 10;
            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new DreamGuard.BE.DAL.ModelExtensions.PaginatedList<CheckoutProductOrder>(items, totalCount, pageNumber, pageSize);
        }
    }
}
