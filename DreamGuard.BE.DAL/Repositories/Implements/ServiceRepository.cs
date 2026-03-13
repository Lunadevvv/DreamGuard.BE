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
    public class ServiceRepository : GenericRepository<Service>, IServiceRepository
    {
        public ServiceRepository(DreamGuardContext context) : base(context)
        {
        }

        public async Task<Service?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Services
                    .Include(s => s.ServiceAssets)
                    .FirstOrDefaultAsync(s => s.ServiceId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving service with ID {id}");
            }
        }
        public async Task<PaginatedList<Service>> GetAllAdminAsync(int pageNumber, bool isActive)
        {
            try
            {
                var query = _context.Services.Where(s => s.IsActive == isActive).Include(s => s.ServiceAssets);
                return await PaginatedList<Service>.CreateAsync(query, pageNumber, 4);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving service for admin");
            }
        }

        public async Task<PaginatedList<Service>> GetAllAsync(int pageNumber, bool isActive)
        {
            try
            {
                var query = _context.Services.Where(s => s.IsActive == isActive).Include(s => s.ServiceAssets);
                return await PaginatedList<Service>.CreateAsync(query, pageNumber, 4);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving service for customer");
            }
        }

    }
}
