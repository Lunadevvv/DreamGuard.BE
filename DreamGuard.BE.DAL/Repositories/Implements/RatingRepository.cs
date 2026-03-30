using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.DbContext;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace DreamGuard.BE.DAL.Repositories.Implements
{
    public class RatingRepository : GenericRepository<Rating>, IRatingRepository
    {
        public RatingRepository(DreamGuardContext context) : base(context)
        {
        }

        public async Task<Rating?> GetRatingByIdAsync(Guid ratingId)
        {
            try
            {
                return await _context.Ratings
                    .Include(r => r.Staff)
                    .Include(r => r.ServiceOrder)
                    .FirstOrDefaultAsync(r => r.RatingId == ratingId); 
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving Rating ");
            }
        }
        public async Task<PaginatedList<Rating>> GetRatingsByAdminSearchdAsync(Guid? serviceOrderId, Guid? staffId, int? score, DateTime? createdAt, DateTime? updatedAt, int pageNumber, int pageSize)
        {
            try
            {

                var query = _context.Ratings
                    .Where(r => (r.ServiceOrderId == serviceOrderId || serviceOrderId == null)
                    && (r.StaffId == staffId || staffId == null)
                    && (r.Score == score || score == null)
                    && (createdAt == null || r.CreatedAt >= createdAt.Value)
                    && (updatedAt == null || (r.UpdatedAt != null && r.UpdatedAt.Value >= updatedAt.Value))).OrderByDescending(r => r.CreatedAt);
                return await PaginatedList<Rating>.CreateAsync(query, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving Rating for admin");
            }
        }

        public async Task<PaginatedList<Rating>> GetRatingsByStaffIdAsync(Guid staffId, int pageNumber, int pageSize)
        {
            try
            {
                var query = _context.Ratings
                    .Where(r => r.StaffId == staffId);
                return await PaginatedList<Rating>.CreateAsync(query, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving Rating for staff");
            }
        }
    }
}
