using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface IProductCustomizeTypeRepository : IGenericRepository<ProductCustomizeType>
    {
        public Task<PaginatedList<ProductCustomizeType>> GetAllWithPagingAsync(int pageNumber, int pageSize, List<Guid> exceedProductCustomizeIds);
        Task<List<ProductCustomizeType>> GetByIdsAsync(List<Guid> ids);
        Task<List<Guid>> GetAllCustomizeTypeIds();
    }
}