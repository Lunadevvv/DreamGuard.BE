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
    public interface IRatingService
    {
        Task<Result<PaginatedList<RatingResponse>>> GetRatingsByStaffId(Guid staffId, int pageNumber, int pageSize);
        Task<Result> CreateRatingAsync(Guid serviceOrderId, Guid customerId, RatingCreateRequest request);
        Task<Result> UpdateRatingAsync(Guid ratingId, Guid customerId, RatingUpdateRequest request);

        Task<Result<PaginatedList<RatingResponse>>> GetRatingsByAdminSearchdAsync(Guid? serviceOrderId, Guid? staffId, int? score, DateTime? createdAt, DateTime? updatedAt, int pageNumber, int pageSize);
        Task<Result<RatingResponse>> GetRatingByIdAsync(Guid ratingId);
    }
}
