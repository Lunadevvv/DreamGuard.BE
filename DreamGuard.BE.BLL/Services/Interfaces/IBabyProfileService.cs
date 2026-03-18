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
    public interface IBabyProfileService
    {
        Task<Result<PaginatedList<BabyProfileResponse>>> GetAllAsync(Guid userId, int pageNumber);
        Task<Result<BabyProfileResponse>> GetByIdAsync(Guid userId, Guid babyId);
        Task<Result> CreateAsync(Guid userId, BabyProfile babyProfile);
        Task<Result> UpdateAsync(Guid userId, Guid babyId, BabyProfileUpdateRequest babyProfileRequest);
        Task<Result> RemoveAsync(Guid userId, Guid babyId);
    }
}
