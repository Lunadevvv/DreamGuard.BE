using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.DbContext;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DreamGuard.BE.DAL.Repositories.Implements
{
    public class ProductFeedbackRepository : GenericRepository<ProductFeedback>, IProductFeedbackRepository
    {
        public ProductFeedbackRepository(DreamGuardContext context) : base(context)
        {
        }

        public async Task<PaginatedList<ProductFeedback>> GetVisibleFeedbacksByProductIdAsync(
            Guid productId, int? filterScore, int pageNumber, int pageSize)
        {
            var query = _context.ProductFeedbacks
                .AsNoTracking()
                .Include(f => f.Customer)
                .Where(f => f.ProductId == productId
                         && f.Status == ProductFeedbackStatus.Visible)
                .Where(f => !filterScore.HasValue || f.Score == filterScore.Value)
                .OrderByDescending(f => f.CreatedAt);

            return await PaginatedList<ProductFeedback>.CreateAsync(query, pageNumber, pageSize);
        }

        public async Task<ProductFeedback?> GetFeedbackByIdAsync(Guid feedbackId)
        {
            return await _context.ProductFeedbacks
                .Include(f => f.Product)
                .FirstOrDefaultAsync(f => f.Id == feedbackId);
        }

        public async Task<bool> ExistsAsync(Guid customerId, Guid productId, Guid orderId)
        {
            return await _context.ProductFeedbacks
                .AnyAsync(f => f.CustomerId == customerId
                            && f.ProductId == productId
                            && f.OrderId == orderId);
        }

        public async Task<(int TotalCount, double AverageScore)> GetVisibleRatingStatsByProductIdAsync(Guid productId)
        {
            var stats = await _context.ProductFeedbacks
                .Where(f => f.ProductId == productId && f.Status == ProductFeedbackStatus.Visible)
                .GroupBy(f => f.ProductId)
                .Select(g => new
                {
                    TotalCount = g.Count(),
                    AverageScore = g.Average(f => (double)f.Score)
                })
                .FirstOrDefaultAsync();

            return stats == null ? (0, 0.0) : (stats.TotalCount, stats.AverageScore);
        }
    }
}
