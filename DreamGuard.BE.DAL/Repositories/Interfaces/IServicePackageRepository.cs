using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface IServicePackageRepository : IGenericRepository<ServicePackage>
    {
        Task<ServicePackage?> GetByIdAsync(Guid id);
        Task<PaginatedList<ServicePackage>> GetAllAdminAsync(int pageNumber, int pageSize, ServicePackageStatus status);
        Task<List<ServicePackage>> GetByListIdAsync(List<Guid> servicePackageIds);
        Task<List<ServicePackage>> GetAllByProductTypeIdAsync(Guid productTypeId);
    }
}
