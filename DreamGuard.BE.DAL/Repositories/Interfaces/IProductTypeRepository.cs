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
    public interface IProductTypeRepository : IGenericRepository<ProductType>
    {
        Task<ProductType?> GetByIdAsync(Guid id);
        Task<PaginatedList<ProductType>> GetAllAsync(int pageNumber, int pageSize);
        Task<PaginatedList<ProductType>> GetAllAdminAsync(int pageNumber, int pageSize, bool isActive);

        Task<List<ServicePackageMapping>> GetMappingsByProductTypeIdAsync(Guid productTypeId);
    }
}
