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
        private readonly IComboRepository _comboRepository;
        private readonly ICustomerRepository _customerRepository;

        public FavoriteProductService(
            IFavoriteProductRepository favoriteProductRepository,
            IProductRepository productRepository,
            IComboRepository comboRepository,
            ICustomerRepository customerRepository)
        {
            _favoriteProductRepository = favoriteProductRepository;
            _productRepository = productRepository;
            _comboRepository = comboRepository;
            _customerRepository = customerRepository;
        }

        public async Task<Result> AddToFavoriteAsync(Guid userId, Guid productId)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result.Failure("Customer profile not found.", 404);

            var customerId = customer.CustomerId;

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return Result.Failure("Product not found.", 404);
            }

            var existing = await _favoriteProductRepository.GetByCustomerAndProductAsync(customerId, productId);
            if (existing != null)
            {
                return Result.Failure("Product is already in your favorites.", 400);
            }

            var favorite = new FavoriteProduct
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
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
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result.Failure("Customer profile not found.", 404);

            var customerId = customer.CustomerId;

            var favorite = await _favoriteProductRepository.GetByCustomerAndProductAsync(customerId, productId);
            if (favorite == null)
            {
                return Result.Failure("Product is not in your favorites.", 404);
            }

            await _favoriteProductRepository.RemoveAsync(favorite);
            return Result.Success("Product removed from favorites.");
        }

        public async Task<Result> AddComboToFavoriteAsync(Guid userId, Guid comboId)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result.Failure("Customer profile not found.", 404);

            var customerId = customer.CustomerId;

            var combo = await _comboRepository.GetByIdAsync(comboId);
            if (combo == null)
            {
                return Result.Failure("Combo not found.", 404);
            }

            var existing = await _favoriteProductRepository.GetByCustomerAndComboAsync(customerId, comboId);
            if (existing != null)
            {
                return Result.Failure("Combo is already in your favorites.", 400);
            }

            var favorite = new FavoriteProduct
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                ProductId = null,
                ComboId = comboId,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _favoriteProductRepository.CreateAsync(favorite);
            if (result <= 0)
            {
                return Result.Failure("Failed to add combo to favorites.", 500);
            }

            return Result.Success("Combo added to favorites.");
        }

        public async Task<Result> RemoveComboFromFavoriteAsync(Guid userId, Guid comboId)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result.Failure("Customer profile not found.", 404);

            var customerId = customer.CustomerId;

            var favorite = await _favoriteProductRepository.GetByCustomerAndComboAsync(customerId, comboId);
            if (favorite == null)
            {
                return Result.Failure("Combo is not in your favorites.", 404);
            }

            await _favoriteProductRepository.RemoveAsync(favorite);
            return Result.Success("Combo removed from favorites.");
        }

        public async Task<Result<PaginatedList<FavoriteProductResponse>>> GetFavoriteProductsAsync(
            Guid userId, int pageNumber)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result<PaginatedList<FavoriteProductResponse>>.Failure("Customer profile not found.", 404);

            var customerId = customer.CustomerId;

            var favorites = await _favoriteProductRepository.GetFavoritesByCustomerIdAsync(customerId, pageNumber);

            var responses = favorites.Items.Select(fp =>
            {
                if (fp.ComboId.HasValue && fp.Combo != null)
                {
                    return new FavoriteProductResponse
                    {
                        Id = fp.Id,
                        ProductId = null,
                        ComboId = fp.ComboId,
                        ItemType = "Combo",
                        ProductName = fp.Combo.Name,
                        Slug = fp.Combo.Slug,
                        BasePrice = fp.Combo.BasePrice,
                        SalePrice = fp.Combo.SalePrice,
                        AverageRating = fp.Combo.AverageRating,
                        Status = fp.Combo.Status.ToString(),
                        ImageUrls = string.IsNullOrEmpty(fp.Combo.ImageUrl) ? new List<string>() : new List<string> { fp.Combo.ImageUrl },
                        CreatedAt = fp.CreatedAt
                    };
                }
                else
                {
                    return new FavoriteProductResponse
                    {
                        Id = fp.Id,
                        ProductId = fp.ProductId,
                        ComboId = null,
                        ItemType = "Product",
                        ProductName = fp.Product?.Name ?? string.Empty,
                        Slug = fp.Product?.Slug ?? string.Empty,
                        BasePrice = fp.Product?.Variants?.FirstOrDefault()?.BasePrice ?? 0,
                        SalePrice = fp.Product?.Variants?.FirstOrDefault()?.SalePrice ?? 0,
                        AverageRating = fp.Product?.AverageRating ?? 0,
                        Status = fp.Product?.Status.ToString() ?? string.Empty,
                        ImageUrls = fp.Product?.Assets?.Select(a => a.Url).ToList() ?? new List<string>(),
                        CreatedAt = fp.CreatedAt
                    };
                }
            }).ToList();

            return Result<PaginatedList<FavoriteProductResponse>>.Success(
                new PaginatedList<FavoriteProductResponse>(
                    responses, favorites.TotalCount, favorites.PageNumber, favorites.PageSize));
        }
    }
}
