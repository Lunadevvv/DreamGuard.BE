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
        Task<Result> ForcedCancelShippingForTradeInAsync(Guid taskId, Guid staffId, FailShippingRequest request); //forced-cancelled-TradeIn
        Task<Result> FailShippingForTradeInAsync(Guid taskId, Guid staffId, FailShippingRequest request); //returned-for-TradeIn
        Task<Result> CompleteShippingForTradeInOrderAsync(Guid taskId, Guid staffId, CompleteShippingRequest request); // delivered-for-tradeIn
        Task<Result> UpdateTaskToDeliveringForTradeInAsync(Guid taskId, Guid staffId, StartShippingRequest request); //delivering-for-tradeIn
        Task<Result> ProcessReturnedTradeInOrderAsync(Guid taskId, ProcessReturnedTradeInRequest request); //process-returned-for-tradeIn
        Task<Result> ProcessExchangeTradeInOrderAsync(Guid taskId, ProcessExchangeTradeInRequest request); //process-exchange-for-tradeIn
    }
}
