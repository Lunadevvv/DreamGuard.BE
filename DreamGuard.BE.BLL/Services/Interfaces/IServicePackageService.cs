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
    public interface IServicePackageService
    {
        Task<Result<PaginatedList<ServicePackageResponse>>> GetAllByAdminAsync(int pageNumber, bool isActive);
        Task<Result<ServicePackageResponse>> GetByIdAsync(Guid servicePackageId);

        
        Task<Result> CreateAsync(ServicePackageCreateRequest servicePackage);
        Task<Result> ReplaceImagesAsync(Guid servicePackageId, PackageImageUploadRequest file);
        Task<Result> DeleteImageAsync(Guid servicePackageId);
        Task<Result> UpdateAsync(Guid servicePackageId, ServicePackageUpdateRequest servicePackageRequest);
        Task<Result> ToggleActiveAsync(Guid servicePackageId);


    }
}
