using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.DAL.ModelExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IServiceEvidenceService
    {
        public Task<Result> CreateAsync(Guid staffId, ServiceEvidenceCreateRequest createRequest);
        public Task<Result<ServiceEvidenceResponse>> GetByIdAsync(Guid serviceEvidenceId);
        public Task<Result<PaginatedList<ServiceEvidenceResponse>>> GetAllAsync(Guid staffId, int pagenumber, int pageSize, Guid serviceTaskId);
        public Task<Result<PaginatedList<ServiceEvidenceResponse>>> AdminSearchSeAsync(int pageNumber, int pageSize, AdminSearchSeRequest adminSearchSeRequest);
    }
}
