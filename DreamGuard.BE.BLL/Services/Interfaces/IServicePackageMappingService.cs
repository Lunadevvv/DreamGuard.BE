using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IServicePackageMappingService
    {
        Task<Result<ServicePackageMappingResponse>> GetByIdAsync(Guid servicePackageMappingId);
        Task<Result<PaginatedList<ServicePackageMappingResponse>>> GetAllAsync(int pageNumber, int pageSize);
        Task<Result> UpdateByIdAsync(Guid servicePackageMappingId, ServicePackageMappingUpdateRequest servicePackageMappingUpdateRequest);
        Task<Result<ServicePackageMappingResponse>> GetByProductTypeIdAndServicePackageIdAsync(Guid productTypeId, Guid servicePackageId);
    }
}
