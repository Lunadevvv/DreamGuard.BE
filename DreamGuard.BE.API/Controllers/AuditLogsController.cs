using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DreamGuard.BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;
        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }
        [HttpGet]
        [Authorize(Roles = $"{Role.Admin}, {Role.Manager}")]
        public async Task<IActionResult> GetAuditLogs(Guid? userId, DateTime? createdAt, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _auditLogService.GetAuditLogsAsync(userId, createdAt, pageNumber, pageSize);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok(result.Data);
        }
    }
}