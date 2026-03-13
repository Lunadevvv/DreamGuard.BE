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
        Task<bool> CheckMappingExistAsync(Guid serviceId, List<Guid> servicePackageIds);
        Task<List<ServicePackageMapping>> GetListByIdAsync(Guid serviceId, List<Guid> servicePackageIds);
    }
}
