using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IServiceTaskService
    {
        public Task<Result> CreateAsync(ServiceTaskCreateRequest serviceTaskCreateRequest);
        public Task<Result<ServiceTaskDetailResponse>> GetByIdAsync(Guid serviceTaskId, Guid staffId, string role);
  
        Task<Result<PaginatedList<ServiceTaskResponse>>> GetBySoIdAsync(Guid soId, int pageNumber, int pageSize);
        Task<Result<PaginatedList<ServiceTaskResponse>>> SearchAsync(AdminSearchServiceTaskRequest searchRequest, int pageNumber, int pageSize);
        Task<Result<PaginatedList<ServiceTaskResponse>>> GetByStaffIdAsync(Guid staffId, int pageNumber, int pageSize);
        Task<Result> UpdateCompletedStatusAsync(Guid serviceTaskId);
        Task<Result> UpdateCheckedOutStatusAsync(Guid serviceTaskId, Guid staffId);
        Task<Result> UpdateProcessingStatusAsync(Guid serviceTaskId, Guid staffId);
        Task<Result> UpdateCheckedInStatusAsync(Guid serviceTaskId, Guid staffId);
        Task<Result> UpdateForcedCancelledStatusAsync(Guid serviceTaskId, Guid staffId, string staffNote);
    }
}
