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
        public async Task<bool> CheckMappingExistAsync(Guid productTypeId, List<Guid> servicePackageIds)
        {
            return await _context.ServicePackageMappings.AnyAsync(spm => servicePackageIds.Contains(spm.ServicePackageId) && spm.ProductTypeId == productTypeId);
        }
        public async Task<List<ServicePackageMapping>> GetListByIdAsync(Guid productTypeId, List<Guid> servicePackageIds)
        {
            return await _context.ServicePackageMappings.Where(spm => servicePackageIds.Contains(spm.ServicePackageId) && spm.ProductTypeId == productTypeId).ToListAsync();
        }
        public async Task<PaginatedList<ServicePackageMapping>> GetAllByAdminAsync(int pageNumber, int pageSize)
        {
            try
            {
                var query = _context.ServicePackageMappings
                    .Include(spm => spm.ServicePackage)
                    .Include(spm => spm.ProductType);
                return await PaginatedList<ServicePackageMapping>.CreateAsync(query, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving service package mappings for admin");
            }
        }

        public async Task<List<ServicePackageMapping>> GetListByProductTypeAndServicePackageAsync(List<Guid> productTypeIds, Guid servicePackageId)
        {
            try
            {
                return await _context.ServicePackageMappings.Where(spm => productTypeIds.Contains(spm.ProductTypeId) && spm.ServicePackageId == servicePackageId).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving List service package mappings");
            }
        }

        public async Task<List<ServicePackageMapping>> GetByListIdAsync(List<Guid> servicePackageMappingIds)
        {
            try
            {
                return await _context.ServicePackageMappings.Include(spm => spm.ProductType).Where(spm => servicePackageMappingIds.Contains(spm.ServicePackageMappingId)).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving List service package mappings");
            }
        }
        public async Task<ServicePackageMapping?> GetByIdAsync(Guid servicePackageMappingId)
        {
            try
            {
                return await _context.ServicePackageMappings
                    .Include(spm => spm.ProductType)
                    .Include(spm => spm.ServicePackage)
                    .FirstOrDefaultAsync(spm => spm.ServicePackageMappingId == servicePackageMappingId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving service package mapping with ID {servicePackageMappingId}");
            }
        }

        public async Task<ServicePackageMapping?> GetByProductTypeIdAndServicePackageIdAsync(Guid productTypeId, Guid servicePackageId)
        {
            try
            {
                return await _context.ServicePackageMappings
                    .Include(spm => spm.ProductType)
                    .Include(spm => spm.ServicePackage)
                    .FirstOrDefaultAsync(spm => spm.ProductTypeId == productTypeId && spm.ServicePackageId == servicePackageId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving service package mapping");
            }
        }
    }
}
