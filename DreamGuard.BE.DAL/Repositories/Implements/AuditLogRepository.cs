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
    public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(DreamGuardContext context) : base(context)
        {
        }

        public async Task<PaginatedList<AuditLog>> GetAuditLogsAsync(Guid? userId, DateTime? createdAt, int pageNumber, int pageSize)
        {
            var query = _context.AuditLogs
                .Where(a => (!userId.HasValue || a.UserId == userId) &&
                            (!createdAt.HasValue || a.CreatedAt.Date >= createdAt.Value.Date))
                .OrderByDescending(a => a.CreatedAt);
            return await PaginatedList<AuditLog>.CreateAsync(query, pageNumber, pageSize);
        }
    }
}
