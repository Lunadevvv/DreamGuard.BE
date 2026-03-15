using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
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
        Task<Result> CreateAsync(StaffCreateRequest StaffCreateRequest);
    }
}
