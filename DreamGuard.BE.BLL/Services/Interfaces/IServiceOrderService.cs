using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IServiceOrderService
    {
        Task<Result<OrderServiceResponse>> OrderServiceAsync(ServiceOrderCreateRequest serviceOrderRequest, Guid customerId, string ipAddress);
        Task<Result> UploadServiceAsset(Guid serviceOrderId, ServiceAssetCreateRequest assetCreateRequest);
        Task<Result<PaginatedList<ServiceOrderResponse>>> GetAllAsync(int pageNumber, int pageSize);
        Task<Result<PaginatedList<ServiceOrderResponse>>> GetAllByAdminAsync(int pageNumber, int pageSize, ServiceOrderSearchRequest searchRequest);
        Task<Result<ServiceOrderDetailResponse>> GetByIdAsync(Guid serviceOrderId, Guid customerId, string role);
        Task<Result> UpdateServiceOrderAsync(Guid customerId, Guid serviceOrderId, ServiceOrderUpdateRequest updateRequest);
        Task<Result> RejectPendingServiceOrderAsync(Guid serviceOrderId);
        Task<Result> ConfirmPendingServiceOrderAsync(Guid serviceOrderId);
        Task<Result> CancelPendingServiceOrderAsync(Guid customerId, Guid serviceOrderId);
        Task<Result> ManagerCancelConfirmedServiceOrderAsync(Guid serviceOrderId);
        Task<Result> ManagerCancelProcessingServiceOrderAsync(Guid serviceOrderId);
        Task<Result<OrderServiceResponse>> ReOrderServiceAsync(Guid SoId, Guid customerId, string ipAddress);
       

    }
}
