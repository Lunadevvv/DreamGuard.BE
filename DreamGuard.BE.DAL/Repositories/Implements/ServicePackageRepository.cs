using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
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
                return await _context.ServicePackages.Where(sp => servicePackageIds.Contains(sp.ServicePackageId) && sp.Status == ServicePackageStatus.Active).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving List ServicePackage");
            }
        }
        public async Task<PaginatedList<ServicePackage>> GetAllAdminAsync(int pageNumber, int pageSize, ServicePackageStatus status)
        {
            try
            {
                var query = _context.ServicePackages.Where(s => s.Status == status);
                return await PaginatedList<ServicePackage>.CreateAsync(query, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving ServicePackage for customer");
            }
        }
        public async Task<List<ServicePackage>> GetAllByProductTypeIdsAsync(List<Guid> productTypeIds)
        {
            try
            {
                // chỉ lấy những servicepackage mà có mapping với tất cả producttypeid trong list
                return await _context.ServicePackages
                    .Where(s => productTypeIds.All(pt => s.ServicePackageMappings.Any(spm => spm.ProductTypeId == pt))
                    ).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving ServicePackage");
            }
        }

    }
}
