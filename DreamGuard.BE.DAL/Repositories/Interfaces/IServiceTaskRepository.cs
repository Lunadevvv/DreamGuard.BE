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
    public interface IServiceTaskRepository : IGenericRepository<ServiceTask>
    {
        public Task<ServiceTask?> GetByIdWithDetailsAsync(Guid serviceTaskId);
        Task<PaginatedList<ServiceTask>> SearchAsync(Guid? staffId, Guid? soId, int pageNumber, int pageSize);
        Task<PaginatedList<ServiceTask>> GetByStaffIdAsync(Guid staffId, int pageNumber, int pageSize);
        Task<PaginatedList<ServiceTask>> GetBySoIdAsync(Guid soId, int pageNumber, int pageSize);
        Task<ServiceTask?> GetByIdWithSoAsync(Guid serviceTaskId);
    }
}
