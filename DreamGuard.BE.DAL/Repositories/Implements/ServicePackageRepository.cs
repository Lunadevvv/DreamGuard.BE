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
    public class ServicePackageRepository : GenericRepository<ServicePackage>, IServicePackageRepository
    {
        public ServicePackageRepository(DreamGuardContext context) : base(context)
        {
        }

        public async Task<ServicePackage?> GetByIdAsync(Guid servicePackageId)
        {
            try
            {
                return await _context.ServicePackages
                    .FirstOrDefaultAsync(s => s.ServicePackageId == servicePackageId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving ServicePackage with ID {servicePackageId}");
            }
        }
        public async Task<List<ServicePackage>> GetByListIdAsync(List<Guid> servicePackageIds)
        {
            try
            {
                return await _context.ServicePackages.Where(sp => servicePackageIds.Contains(sp.ServicePackageId) && sp.IsActive).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving List ServicePackage");
            }
        }
        public async Task<PaginatedList<ServicePackage>> GetAllAdminAsync(int pageNumber, int pageSize, bool isActive)
        {
            try
            {
                var query = _context.ServicePackages.Where(s => s.IsActive == isActive);
                return await PaginatedList<ServicePackage>.CreateAsync(query, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving ServicePackage for customer");
            }
        }
        public async Task<List<ServicePackage>> GetAllByServiceIdAsync(Guid serviceId)
        {
            try
            {
                //lấy servicepackage có mapping với serviceId
                return await _context.ServicePackages.Where(s => s.ServicePackageMappings.Any(spm => spm.ServiceId == serviceId)).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving ServicePackage");
            }
        }

    }
}
