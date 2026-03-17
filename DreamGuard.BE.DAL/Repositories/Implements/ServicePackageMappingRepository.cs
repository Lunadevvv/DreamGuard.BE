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
                var query = _context.ServicePackageMappings;
                return await PaginatedList<ServicePackageMapping>.CreateAsync(query, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving service package mappings for admin");
            }
        }
    }
}
