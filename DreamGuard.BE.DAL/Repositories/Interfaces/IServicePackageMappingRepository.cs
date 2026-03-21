using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface IServicePackageMappingRepository : IGenericRepository<ServicePackageMapping>
    {
        Task<List<ServicePackageMapping>> GetListByProductTypeAndServicePackageAsync(List<Guid> productTypeIds, Guid servicePackageId);
        Task<List<ServicePackageMapping>> GetByListIdAsync(List<Guid> servicePackageMappingIds);
        Task<bool> CheckMappingExistAsync(Guid productTypeId, List<Guid> servicePackageIds);
        Task<List<ServicePackageMapping>> GetListByIdAsync(Guid productTypeId, List<Guid> servicePackageIds);
        Task<PaginatedList<ServicePackageMapping>> GetAllByAdminAsync(int pageNumber, int pageSize);
        Task<ServicePackageMapping?> GetByIdAsync(Guid servicePackageMappingId);
        Task<ServicePackageMapping?> GetByProductTypeIdAndServicePackageIdAsync(Guid productTypeId, Guid servicePackageId);
    }
}
