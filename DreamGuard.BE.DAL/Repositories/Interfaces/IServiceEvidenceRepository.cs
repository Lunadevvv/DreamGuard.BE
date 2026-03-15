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
    public interface IServiceEvidenceRepository : IGenericRepository<ServiceEvidence>
    {
        Task<PaginatedList<ServiceEvidence>> AdminSearchSeAsync(int pageNumber, int pageSize, Guid? serviceEvidenceId, Guid? serviceTaskId);
        Task<PaginatedList<ServiceEvidence>> GetAllAsync(int pageNumber, int pageSize, Guid serviceTaskId);
    }
}
