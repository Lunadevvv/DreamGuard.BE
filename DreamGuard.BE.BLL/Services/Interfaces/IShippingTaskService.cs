using System;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IShippingTaskService
    {
        Task<Result<ShippingTaskResponse>> CreateShippingTaskAsync(ShippingTaskCreateRequest request);
        Task<Result> ReassignStaffAsync(Guid taskId, ReassignStaffRequest request);
        Task<Result> UpdateTaskToDeliveringAsync(Guid taskId, Guid staffId, StartShippingRequest request);
        Task<Result> UpdateTaskToArrivedAsync(Guid taskId, Guid staffId);
        Task<Result> CompleteShippingAsync(Guid taskId, Guid staffId, CompleteShippingRequest request);
        Task<Result> FailShippingAsync(Guid taskId, Guid staffId, FailShippingRequest request);
        Task<Result> ProcessReturnedOrderAsync(Guid taskId, ProcessReturnedRequest request);
        Task<Result> ProcessExchangeOrderAsync(Guid taskId, ProcessExchangeRequest request);
        
        Task<Result<ShippingTaskResponse>> GetTaskByIdAsync(Guid taskId);
        Task<Result<PaginatedList<ShippingTaskResponse>>> GetAllTasksForAdminAsync(int pageNumber, string? status = null, Guid? orderId = null );
        Task<Result<PaginatedList<ShippingTaskResponse>>> GetTasksForStaffAsync(Guid staffId, int pageNumber = 1);
    }
}
