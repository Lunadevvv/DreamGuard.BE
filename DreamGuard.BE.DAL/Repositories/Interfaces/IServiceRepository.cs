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
    public interface IServiceRepository : IGenericRepository<Service>
    {
        Task<Service?> GetByIdAsync(Guid id);
        Task<PaginatedList<Service>> GetAllAsync(int pageNumber, int pageSize);
        Task<PaginatedList<Service>> GetAllAdminAsync(int pageNumber, int pageSize, bool isActive);

        Task<List<ServicePackageMapping>> GetMappingsByServiceIdAsync(Guid serviceId);
    }
}
