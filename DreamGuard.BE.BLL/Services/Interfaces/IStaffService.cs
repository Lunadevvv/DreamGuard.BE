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
    public interface IStaffService
    {
        Task<Result> UpdateAsync(Guid staffId, StaffUpdateRequest staffUpdateRequest);
        Task<Result<StaffResponse>> GetByIdAsync(Guid StaffId);
        Task<Result<PaginatedList<StaffResponse>>> GetAllAsync(int pageNumber, int pageSize);
        Task<Result> UpdateRoleAsync(Guid staffId, string newRole);
        Task<Result> UpdateAccountAsync(Guid staffId, StaffAccountUpdateRequest staffUpdateRequest);
        Task<Result<PaginatedList<RatingResponse>>> GetRatings(Guid staffId, int pageNumber, int pageSize);
        Task<Result<PaginatedList<StaffResponse>>> GetCleaningStaffsForAssignmentAsync(int pageNumber, int pageSize);
        Task<Result<PaginatedList<StaffResponse>>> GetDeliveryStaffsForAssignmentAsync(int pageNumber, int pageSize);
    }
}
