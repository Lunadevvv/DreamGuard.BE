using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.DAL.ModelExtensions;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IProductFeedbackService
    {
        Task<Result> CreateFeedbackAsync(Guid orderItemId, Guid customerId,
            ProductFeedbackCreateRequest request);

        Task<Result<PaginatedList<ProductFeedbackResponse>>> GetFeedbacksByProductIdAsync(
            Guid productId, int? filterScore, int pageNumber, int pageSize);

        Task<Result> UpdateFeedbackStatusAsync(Guid feedbackId, string status);
    }
}
