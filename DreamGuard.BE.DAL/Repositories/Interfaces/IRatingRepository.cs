using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface IRatingRepository : IGenericRepository<Rating>
    {
        Task<PaginatedList<Rating>> GetRatingsByStaffIdAsync(Guid staffId, int pageNumber, int pageSize);
        Task<PaginatedList<Rating>> GetRatingsByAdminSearchdAsync(Guid? serviceOrderId, Guid? staffId, int? score, DateTime? createdAt, DateTime? updatedAt, int pageNumber, int pageSize);
        Task<Rating?> GetRatingByIdAsync(Guid ratingId);
    }
}
