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
    public class SystemConfigRepository : GenericRepository<SystemConfig>, ISystemConfigRepository
    {
        public SystemConfigRepository(DreamGuardContext context) : base(context)
        {
        }

        public async Task<SystemConfig?> GetByKeyAsync(string key)
        {
            return await _context.SystemConfigs.FirstOrDefaultAsync(x => x.ConfigKey == key);
        }

        public async Task<PaginatedList<SystemConfig>> GetAllConfigsAsync(int pageNumber, int pageSize)
        {
            var query = _context.SystemConfigs.AsNoTracking().OrderBy(x => x.ConfigKey);
            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<SystemConfig>(items, totalCount, pageNumber, pageSize);
        }
    }
}
