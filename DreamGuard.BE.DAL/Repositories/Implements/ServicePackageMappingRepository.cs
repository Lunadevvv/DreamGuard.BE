using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.DbContext;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Repositories.Implements
{
    public class ServicePackageMappingRepository : GenericRepository<ServicePackageMapping>, IServicePackageMappingRepository
    {
        public ServicePackageMappingRepository(DreamGuardContext context) : base(context)
        {
        }
        public async Task<bool> CheckMappingExistAsync(Guid serviceId, List<Guid> servicePackageIds)
        {
            return await _context.ServicePackageMappings.AnyAsync(spm => servicePackageIds.Contains(spm.ServicePackageId) && spm.ServiceId == serviceId);
        }
        public async Task<List<ServicePackageMapping>> GetListByIdAsync(Guid serviceId, List<Guid> servicePackageIds)
        {
            return await _context.ServicePackageMappings.Where(spm => servicePackageIds.Contains(spm.ServicePackageId) && spm.ServiceId == serviceId).ToListAsync();
        }
    }
}
