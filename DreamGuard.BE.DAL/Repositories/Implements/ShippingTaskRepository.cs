using System;
using System.Collections.Generic;
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
    public class ShippingTaskRepository : GenericRepository<ShippingTask>, IShippingTaskRepository
    {
        public ShippingTaskRepository(DreamGuardContext context) : base(context)
        {
        }

        public async Task<ShippingTask?> GetTaskWithDetailsAsync(Guid taskId)
        {
            return await _context.ShippingTasks
                .Include(t => t.Staff)
                .Include(t => t.Order)
                .Include(t => t.ShippingEvidences)
                .Include(t => t.TradeInOrder)
                .FirstOrDefaultAsync(t => t.ShippingTaskId == taskId);
        }

        public async Task<ShippingTask?> GetTaskWithDetailsForUpdateAsync(Guid taskId)
        {
            //please commit this
            return await _context.ShippingTasks
                .Include(t => t.Staff)
                .Include(t => t.Order)
                .Include(t => t.ShippingEvidences)
                .Include(t => t.TradeInOrder)
                    .ThenInclude(ti => ti.ProductVariant)
                .Include(t => t.TradeInOrder)
                    .ThenInclude(ti => ti.Payments)
                .AsTracking()
                .FirstOrDefaultAsync(t => t.ShippingTaskId == taskId);
        }
        public async Task<ShippingTask?> GetTaskWithDetailsForUpdateNoTrackingAsync(Guid taskId)
        {
            return await _context.ShippingTasks
                .Include(t => t.Staff)
                .Include(t => t.Order)
                .Include(t => t.ShippingEvidences)
                .Include(t => t.TradeInOrder)
                // TradeInOrder + c�c navigation b�n trong
                .Include(t => t.TradeInOrder)
                    .ThenInclude(ti => ti.Payments)
                .Include(t => t.TradeInOrder)
                    .ThenInclude(ti => ti.ProductVariant)
                        .ThenInclude(pv => pv.Inventory)
                .Include(t => t.TradeInOrder)
                    .ThenInclude(ti => ti.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Include(t => t.TradeInOrder)
                    .ThenInclude(ti => ti.OrderItem)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.ShippingTaskId == taskId);
        }


        public async Task<List<ShippingTask>> GetTaskByOrderIdAsync(Guid orderId)
        {
            return await _context.ShippingTasks
                .OrderByDescending(t => t.CreatedAt)
                .Where(t => t.OrderId == orderId).ToListAsync();
        }

        public async Task<PaginatedList<ShippingTask>> GetTasksByStaffIdAsync(Guid staffId, int pageNumber = 1)
        {
            var query = _context.ShippingTasks
                .Include(t => t.Order)
                .Include(t => t.TradeInOrder)
                .Where(t => t.StaffId == staffId)
                .OrderByDescending(t => t.CreatedAt)
                .AsQueryable();

            return await PaginatedList<ShippingTask>.CreateAsync(query, pageNumber, 10);
        }

        public async Task<PaginatedList<ShippingTask>> GetAllTasksForAdminAsync(int pageNumber, string? status, Guid? orderId, Guid? tradeInOrderId)
        {
            var query = _context.ShippingTasks
                .Include(t => t.Staff)
                .Include(t => t.Order)
                .Include(t => t.TradeInOrder)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status == status);
            }
            if(orderId.HasValue)
            {
                query = query.Where(t => t.OrderId == orderId.Value);
            }
            if(tradeInOrderId.HasValue)
            {
                query = query.Where(t => t.TradeInOrderId == tradeInOrderId.Value);
            }

            query = query.OrderByDescending(t => t.ShippingDate ?? t.CompletionDate ?? DateTime.UtcNow);

            return await PaginatedList<ShippingTask>.CreateAsync(query, pageNumber, 10);
        }




    }
}
