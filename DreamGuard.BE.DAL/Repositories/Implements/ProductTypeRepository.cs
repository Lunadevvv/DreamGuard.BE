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
    public class ProductTypeRepository : GenericRepository<ProductType>, IProductTypeRepository
    {
        public ProductTypeRepository(DreamGuardContext context) : base(context)
        {
        }

        public async Task<ProductType?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.ProductTypes
                    .FirstOrDefaultAsync(s => s.ProductTypeId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving ProductType with ID {id}");
            }
        }
        public async Task<List<ServicePackageMapping>> GetMappingsByProductTypeIdAsync(Guid productTypeId)
        {
            try
            {
                return await _context.ServicePackageMappings
                    .Include(spm => spm.ProductType)
                    .Include(spm => spm.ServicePackage)
                    .Where(spm => spm.ProductTypeId == productTypeId).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving mappings for ProductType with ID {productTypeId}");
            }
        }
        public async Task<PaginatedList<ProductType>> GetAllAdminAsync(int pageNumber, int pageSize, bool isActive)
        {
            try
            {
                var query = _context.ProductTypes.Where(s => s.IsActive == isActive);
                return await PaginatedList<ProductType>.CreateAsync(query, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving ProductType for admin");
            }
        }

        public async Task<PaginatedList<ProductType>> GetAllAsync(int pageNumber, int pageSize, List<Guid> exceedProductTypeIds)
        {
            try
            {
                var query = _context.ProductTypes.Where(s => !exceedProductTypeIds.Contains(s.ProductTypeId) && s.IsActive);
                return await PaginatedList<ProductType>.CreateAsync(query, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving ProductType for customer");
            }
        }

    }
}
