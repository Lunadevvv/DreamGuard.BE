using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Implements;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _repo;
        private readonly IHubService _hubService;
        public AuditLogService(IAuditLogRepository repo, IHubService hubService)
        {
            _repo = repo;
            _hubService = hubService;
        }

        public async Task<Result<PaginatedList<AuditLog>>> GetAuditLogsAsync(Guid? userId, DateTime? createdAt, int pageNumber, int pageSize)
        {
            var auditLogs = await _repo.GetAuditLogsAsync(userId, createdAt, pageNumber, pageSize);
            return Result<PaginatedList<AuditLog>>.Success(auditLogs);
        }

        public async Task LogAsync(AuditLog audit)
        {
            await _repo.CreateAsync(audit);
            await _hubService.SendAuditLogToAdmins(audit);
        }
    }
}
