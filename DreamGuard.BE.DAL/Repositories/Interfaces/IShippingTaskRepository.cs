using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Models;

using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Constants;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface IShippingTaskRepository : IGenericRepository<ShippingTask>
    {

        Task<ShippingTask?> GetTaskWithDetailsAsync(Guid taskId);
        Task<ShippingTask?> GetTaskWithDetailsForUpdateAsync(Guid taskId);
        Task<ShippingTask?> GetTaskWithDetailsForUpdateNoTrackingAsync(Guid taskId);
        Task<List<ShippingTask>> GetTaskByOrderIdAsync(Guid orderId);
        Task<PaginatedList<ShippingTask>> GetTasksByStaffIdAsync(Guid staffId, int pageNumber = 1);
        Task<PaginatedList<ShippingTask>> GetAllTasksForAdminAsync(int pageNumber, string? status, Guid? orderId, Guid? tradeInOrderId);

    }
}
