using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class FavoriteProductService : IFavoriteProductService
    {
        private readonly IFavoriteProductRepository _favoriteProductRepository;
        private readonly IProductRepository _productRepository;

        public FavoriteProductService(
            IFavoriteProductRepository favoriteProductRepository,
            IProductRepository productRepository)
        {
            _favoriteProductRepository = favoriteProductRepository;
            _productRepository = productRepository;
        }

        public async Task<Result> AddToFavoriteAsync(Guid userId, Guid productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return Result.Failure("Product not found.", 404);
            }

            var existing = await _favoriteProductRepository.GetByUserAndProductAsync(userId, productId);
            if (existing != null)
            {
                return Result.Failure("Product is already in your favorites.", 400);
            }

            var favorite = new FavoriteProduct
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _favoriteProductRepository.CreateAsync(favorite);
            if (result <= 0)
            {
                return Result.Failure("Failed to add product to favorites.", 500);
            }

            return Result.Success("Product added to favorites.");
        }

        public async Task<Result> RemoveFromFavoriteAsync(Guid userId, Guid productId)
        {
            var favorite = await _favoriteProductRepository.GetByUserAndProductAsync(userId, productId);
            if (favorite == null)
            {
                return Result.Failure("Product is not in your favorites.", 404);
            }

            await _favoriteProductRepository.RemoveAsync(favorite);
            return Result.Success("Product removed from favorites.");
        }

        public async Task<Result<PaginatedList<FavoriteProductResponse>>> GetFavoriteProductsAsync(
            Guid userId, int pageNumber)
        {
            var favorites = await _favoriteProductRepository.GetFavoritesByUserIdAsync(userId, pageNumber);

            var responses = favorites.Items.Select(fp => new FavoriteProductResponse
            {
                Id = fp.Id,
                ProductId = fp.ProductId,
                ProductName = fp.Product?.Name ?? string.Empty,
                Slug = fp.Product?.Slug ?? string.Empty,
                BasePrice = fp.Product?.Variants?.FirstOrDefault()?.BasePrice ?? 0,
                SalePrice = fp.Product?.Variants?.FirstOrDefault()?.SalePrice ?? 0,
                AverageRating = fp.Product?.AverageRating ?? 0,
                Status = fp.Product?.Status.ToString() ?? string.Empty,
                ImageUrls = fp.Product?.Assets?.Select(a => a.Url).ToList() ?? new List<string>(),
                CreatedAt = fp.CreatedAt
            }).ToList();

            return Result<PaginatedList<FavoriteProductResponse>>.Success(
                new PaginatedList<FavoriteProductResponse>(
                    responses, favorites.TotalCount, favorites.PageNumber, favorites.PageSize));
        }
    }
}
