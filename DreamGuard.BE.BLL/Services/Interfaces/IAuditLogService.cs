using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync (AuditLog audit);
        Task<Result<PaginatedList<AuditLog>>> GetAuditLogsAsync(Guid? userId, DateTime? createdAt, int pageNumber, int pageSize);
    }
}
