using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.DbContext;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Repositories.Implements
{
    public class ServiceEvidenceRepository : GenericRepository<ServiceEvidence>, IServiceEvidenceRepository
    {

        public ServiceEvidenceRepository(DreamGuardContext context) : base(context)
        {
        }

        public async Task<PaginatedList<ServiceEvidence>> AdminSearchSeAsync(int pageNumber, int pageSize, Guid? serviceEvidenceId, Guid? serviceTaskId)
        {
            try
            {
                var query = _context.ServiceEvidences.Where(se => (se.ServiceTaskId == serviceTaskId || serviceTaskId == null) && (se.SeId == serviceEvidenceId || serviceEvidenceId == null));
                return await PaginatedList<ServiceEvidence>.CreateAsync(query, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving service evidences: {ex.Message}", ex);
            }
        }

        public async Task<PaginatedList<ServiceEvidence>> GetAllAsync(int pageNumber, int pageSize, Guid serviceTaskId)
        {
            try
            {
                var query = _context.ServiceEvidences.Where(se => se.ServiceTaskId == serviceTaskId);
                return await PaginatedList<ServiceEvidence>.CreateAsync(query, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving service evidences: {ex.Message}", ex);
            }
        }
    }
}