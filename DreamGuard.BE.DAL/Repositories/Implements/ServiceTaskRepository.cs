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
    public class ServiceTaskRepository : GenericRepository<ServiceTask>, IServiceTaskRepository
    {
        public ServiceTaskRepository(DreamGuardContext context) : base(context)
        {
        }

        public async Task<ServiceTask?> GetByIdWithSoAsync(Guid serviceTaskId)
        {
            try
            {
                var serviceTask = await _context.ServiceTasks
                    .Include(st => st.ServiceEvidences)
                    .Include(st => st.ServiceOrder)
                        .ThenInclude(so => so.Payments)
                    .FirstOrDefaultAsync(st => st.ServiceTaskId == serviceTaskId);
                return serviceTask;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving service task: {ex.Message}");
            }
        }
        public async Task<ServiceTask?> GetByIdWithDetailsAsync(Guid serviceTaskId)
        {
            try
            {
                var serviceTask = await _context.ServiceTasks
                    .Include(st => st.ServiceOrder)
                        .ThenInclude(so => so.ServicePackageMapping)
                            .ThenInclude(spm => spm.ServicePackage)
                    .Include(st => st.ServiceOrder)
                        .ThenInclude(so => so.ServicePackageMapping)
                            .ThenInclude(spm => spm.ProductType)
                    .Include(st => st.ServiceOrder)
                        .ThenInclude(so => so.Payments)
                    .FirstOrDefaultAsync(st => st.ServiceTaskId == serviceTaskId);
                return serviceTask;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving service task details: {ex.Message}");
            }
        }

        public async Task<PaginatedList<ServiceTask>> GetBySoIdAsync(Guid soId, int pageNumber, int pageSize)
        {
            try
            {
                var query = _context.ServiceTasks
                    .Where(st => st.SoId == soId);
                return await PaginatedList<ServiceTask>.CreateAsync(query, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving service tasks by service order ID: {ex.Message}");
            }
        }

        public async Task<PaginatedList<ServiceTask>> SearchAsync(Guid? staffId, Guid? SoId, int pageNumber, int pageSize)
        {
            try
            {
                var query = _context.ServiceTasks
                    .Where(st => (st.StaffId == staffId || staffId == null) && (st.SoId == SoId || SoId == null));
                return await PaginatedList<ServiceTask>.CreateAsync(query, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching service task: {ex.Message}");
            }
        }
        public async Task<PaginatedList<ServiceTask>> GetByStaffIdAsync(Guid staffId, int pageNumber, int pageSize)
        {
            try
            {
                var query = _context.ServiceTasks
                    .Where(st => st.StaffId == staffId);
                return await PaginatedList<ServiceTask>.CreateAsync(query, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving service tasks by staffId: {ex.Message}");
            }
        }
    }
}
