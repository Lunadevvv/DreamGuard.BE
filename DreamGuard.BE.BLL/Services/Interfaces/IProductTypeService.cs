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
    public interface IProductTypeService 
    {
        Task<Result<PaginatedList<ProductTypeResponse>>> GetAllByAdminAsync(int pageNumber, int pageSize, bool isActive);
        Task<Result<PaginatedList<ProductTypeResponse>>> GetAllAsync(int pageNumber, int pageSize);
        Task<Result<List<ServicePackageResponse>>> GetPackagesByProductTypeIdsAsync(List<Guid> productTypeIds);
        Task<Result<ProductTypeResponse>> GetByIdAsync(Guid productTypeId);
        Task<Result> CreateAsync(ProductTypeCreateRequest service);
        Task<Result> UpdateAsync(Guid productTypeId, ProductTypeUpdateRequest serviceRequest);
        Task<Result> ToggleActiveAsync(Guid productTypeId);
        Task<Result> AssignPackagesAsync(Guid productTypeId, AssignServicePackagesRequest assignServicePackagesRequest);
        Task<Result> RemovePackagesAsync(Guid productTypeId, RemoveServicePackagesRequest removeServicePackagesRequest);
        Task<Result<List<ServicePackageMappingDetailResponse>>> GetMappingsByProductTypeIdAsync(Guid productTypeId);
    }
}
