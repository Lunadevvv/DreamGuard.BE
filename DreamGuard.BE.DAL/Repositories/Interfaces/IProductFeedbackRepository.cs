using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface IProductFeedbackRepository : IGenericRepository<ProductFeedback>
    {
        Task<PaginatedList<ProductFeedback>> GetVisibleFeedbacksByProductIdAsync(
            Guid productId, int? filterScore, int pageNumber, int pageSize);
        Task<ProductFeedback?> GetFeedbackByIdAsync(Guid feedbackId);
        Task<bool> ExistsAsync(Guid customerId, Guid productId, Guid orderId);
        Task<(int TotalCount, double AverageScore)> GetVisibleRatingStatsByProductIdAsync(Guid productId);
    }
}
