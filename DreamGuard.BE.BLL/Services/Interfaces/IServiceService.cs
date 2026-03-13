using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IServiceService 
    {
        Task<Result<PaginatedList<ServiceResponse>>> GetAllByAdminAsync(int pageNumber, bool isActive);
        Task<Result<List<ServicePackageResponse>>> GetPackagesByServiceIdAsync(Guid serviceId);
        Task<Result<PaginatedList<ServiceResponse>>> GetAllAsync(int pageNumber);
        Task<Result<ServiceResponse>> GetByIdAsync(Guid serviceId);
        Task<Result> CreateAsync(ServiceCreateRequest service);
        Task<Result> AddImagesAsync(Guid serviceId, ImageUploadRequest files);
        Task<Result> DeleteImageAsync(Guid serviceId, Guid assetId);
        Task<Result> UpdateAsync(Guid serviceId, ServiceUpdateRequest serviceRequest);
        Task<Result> ToggleActiveAsync(Guid serviceId);
        Task<Result> AssignPackagesAsync(Guid serviceId, AssignServicePackagesRequest assignServicePackagesRequest);
        Task<Result> RemovePackagesAsync(Guid serviceId, RemoveServicePackagesRequest removeServicePackagesRequest);
    }
}
