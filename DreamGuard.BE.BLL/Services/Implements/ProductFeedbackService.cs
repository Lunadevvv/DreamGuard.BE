using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class ProductFeedbackService : IProductFeedbackService
    {
        private readonly IProductFeedbackRepository _feedbackRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICustomerRepository _customerRepository;
        private readonly ISystemConfigRepository _systemConfigRepository;

        public ProductFeedbackService(
            IProductFeedbackRepository feedbackRepository,
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IUnitOfWork unitOfWork,
            ICustomerRepository customerRepository,
            ISystemConfigRepository systemConfigRepository)
        {
            _feedbackRepository = feedbackRepository;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _customerRepository = customerRepository;
            _systemConfigRepository = systemConfigRepository;
        }

        public async Task<Result> CreateFeedbackAsync(
            Guid orderItemId, Guid customerId,
            ProductFeedbackCreateRequest request)
        {
            // 1. Load OrderItem with Order and ProductVariant
            var orderItem = await _orderRepository.GetOrderItemByIdAsync(orderItemId);
            if (orderItem == null)
            {
                return Result.Failure("Order item not found.", 404);
            }

            // 2. Validate it's a product item (not a combo)
            if (orderItem.ProductVariant == null)
            {
                return Result.Failure("Feedback can only be submitted for product items, not combos.", 400);
            }

            var order = orderItem.Order!;
            var productId = orderItem.ProductVariant.ProductId;
            var orderId = orderItem.OrderId;

            // 3. Validate order belongs to customer
            if (order.CustomerId != customerId)
            {
                return Result.Failure("You can only provide feedback for your own orders.", 403);
            }

            // 4. Validate order is completed
            if (order.Status != OrderStatus.Completed)
            {
                return Result.Failure("Feedback can only be submitted for completed orders.", 400);
            }

            // 5. Check uniqueness
            bool alreadyExists = await _feedbackRepository.ExistsAsync(customerId, productId, orderId);
            if (alreadyExists)
            {
                return Result.Failure("You have already submitted feedback for this product in this order.", 400);
            }

            // 6. Load product for rating update (tracked for SaveChanges)
            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null)
            {
                return Result.Failure("Product not found.", 404);
            }

            // 7. Create feedback entity
            var feedback = new ProductFeedback
            {
                ProductId = productId,
                OrderId = orderId,
                CustomerId = customerId,
                Score = request.Score,
                Comment = request.Comment
            };

            // 8. Incremental rating update (same pattern as Staff.AverageRating)
            product.TotalRating += 1;
            product.AverageRating = ((product.AverageRating * (product.TotalRating - 1)) + request.Score)
                                    / product.TotalRating;

            _feedbackRepository.AddEntity(feedback);
            _productRepository.UpdateEntity(product);

            // Award coins to customer
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer != null)
            {
                var rewardConfig = await _systemConfigRepository.GetByKeyAsync("FeedbackCoinReward");
                int rewardCoins = 50; // default
                if (rewardConfig != null && int.TryParse(rewardConfig.ConfigValue, out int parsedReward))
                {
                    rewardCoins = parsedReward;
                }
                customer.MemberCoin += rewardCoins;
                _customerRepository.UpdateEntity(customer);
            }

            var saved = await _unitOfWork.SaveChangeAsync();
            if (saved == 0)
            {
                return Result.Failure("Failed to save feedback.", 500);
            }

            return Result.Success($"{feedback.Id}");
        }

        public async Task<Result<PaginatedList<ProductFeedbackResponse>>> GetFeedbacksByProductIdAsync(
            Guid productId, int? filterScore, int pageNumber, int pageSize)
        {
            // Repository eagerly includes Customer — no N+1
            var feedbacks = await _feedbackRepository
                .GetVisibleFeedbacksByProductIdAsync(productId, filterScore, pageNumber, pageSize);

            // Manual projection — avoids AutoMapper lazy-loading pitfalls
            var responses = feedbacks.Items.Select(MapToResponse).ToList();

            var paginatedResult = new PaginatedList<ProductFeedbackResponse>(
                responses, feedbacks.TotalCount, feedbacks.PageNumber, feedbacks.PageSize);

            return Result<PaginatedList<ProductFeedbackResponse>>.Success(paginatedResult);
        }

        public async Task<Result> UpdateFeedbackStatusAsync(Guid feedbackId, string status)
        {
            if (status != ProductFeedbackStatus.Visible && status != ProductFeedbackStatus.Hidden)
            {
                return Result.Failure("Invalid status. Must be 'Visible' or 'Hidden'.", 400);
            }

            // GetFeedbackByIdAsync includes Product — no extra query for product update
            var feedback = await _feedbackRepository.GetFeedbackByIdAsync(feedbackId);
            if (feedback == null)
            {
                return Result.Failure("Feedback not found.", 404);
            }

            if (feedback.Status == status)
            {
                return Result.Failure($"Feedback is already {status}.", 400);
            }

            // Update status
            feedback.Status = status;
            feedback.UpdatedAt = DateTime.UtcNow;
            _feedbackRepository.UpdateEntity(feedback);

            // Recalculate rating from visible feedbacks using a single aggregate query
            var (totalCount, averageScore) = await _feedbackRepository
                .GetVisibleRatingStatsByProductIdAsync(feedback.ProductId);

            // Adjust for the status change happening in the current transaction
            if (status == ProductFeedbackStatus.Hidden)
            {
                // This feedback is still Visible in DB, so subtract it
                if (totalCount > 1)
                {
                    totalCount -= 1;
                    averageScore = (averageScore * (totalCount + 1) - feedback.Score) / totalCount;
                }
                else
                {
                    totalCount = 0;
                    averageScore = 0;
                }
            }
            else // Visible — this feedback is still Hidden in DB, so add it
            {
                totalCount += 1;
                averageScore = ((averageScore * (totalCount - 1)) + feedback.Score) / totalCount;
            }

            feedback.Product.TotalRating = totalCount;
            feedback.Product.AverageRating = averageScore;
            _productRepository.UpdateEntity(feedback.Product);

            var saved = await _unitOfWork.SaveChangeAsync();
            if (saved == 0)
            {
                return Result.Failure("Failed to update feedback status.", 500);
            }

            return Result.Success($"Feedback status updated to {status}.");
        }

        private static ProductFeedbackResponse MapToResponse(ProductFeedback feedback)
        {
            return new ProductFeedbackResponse
            {
                Id = feedback.Id,
                ProductId = feedback.ProductId,
                OrderId = feedback.OrderId,
                CustomerName = feedback.Customer?.FullName ?? string.Empty,
                CustomerAvatar = feedback.Customer?.AvatarUrl ?? string.Empty,
                Score = feedback.Score,
                Comment = feedback.Comment,
                Status = feedback.Status,
                CreatedAt = feedback.CreatedAt,
                UpdatedAt = feedback.UpdatedAt
            };
        }
    }
}
