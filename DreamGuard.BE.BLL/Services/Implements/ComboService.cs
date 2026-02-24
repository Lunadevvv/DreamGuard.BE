using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class ComboService : IComboService
    {
        private readonly IComboRepository _comboRepository;
        private readonly IProductVariantRepository _variantRepository;
        private readonly IMapper _mapper;

        public ComboService(
            IComboRepository comboRepository,
            IProductVariantRepository variantRepository,
            IMapper mapper)
        {
            _comboRepository = comboRepository;
            _variantRepository = variantRepository;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedList<ComboResponse>>> GetAllCombosAsync(
            int pageNumber, double? maxPrice, int? maxAgeGroup, string? color)
        {
            var combos = await _comboRepository.GetAllCombosAsync(
                pageNumber, maxPrice, maxAgeGroup, color);

            if (combos == null || !combos.Items.Any())
            {
                return Result<PaginatedList<ComboResponse>>.Failure(
                    "No combos found.", 404);
            }

            var responses = combos.Items.Select(MapToComboResponse).ToList();

            var paginatedResponse = new PaginatedList<ComboResponse>(
                responses, combos.TotalCount, combos.PageNumber, combos.PageSize);

            return Result<PaginatedList<ComboResponse>>.Success(paginatedResponse);
        }

        public async Task<Result<ComboDetailResponse>> GetComboByIdAsync(Guid id)
        {
            var combo = await _comboRepository.GetComboByIdAsync(id);
            if (combo == null)
            {
                return Result<ComboDetailResponse>.Failure("Combo not found.", 404);
            }

            if (combo.ComboParentId == null)
            {
                // Parent combo: load children only
                var parentCombo = await _comboRepository.GetComboWithChildrenAsync(id);
                var response = MapToDetailResponse(parentCombo!);
                response.ChildCombos = parentCombo!.ComboChildrens
                    .Select(ch => new ComboChildResponse
                    {
                        Id = ch.Id,
                        Name = ch.Name,
                        Slug = ch.Slug,
                        AgeGroup = ch.AgeGroup,
                        Color = ch.Color,
                        Size = ch.Size,
                        BasePrice = ch.BasePrice,
                        SalePrice = ch.SalePrice,
                        ImageUrl = ch.ImageUrl,
                        AverageRating = ch.AverageRating
                    }).ToList();
                response.ProductItems = null;
                return Result<ComboDetailResponse>.Success(response);
            }
            else
            {
                // Child combo: load product variants with quantities
                var childCombo = await _comboRepository.GetComboWithProductsAsync(id);
                var response = MapToDetailResponse(childCombo!);
                response.ProductItems = childCombo!.ComboProductVariants
                    .Select(cpv => new ComboProductItemResponse
                    {
                        ProductVariantId = cpv.ProductVariantId,
                        Sku = cpv.ProductVariant?.Sku,
                        ProductName = cpv.ProductVariant?.Product?.Name ?? string.Empty,
                        BasePrice = cpv.ProductVariant?.BasePrice ?? 0,
                        SalePrice = cpv.ProductVariant?.SalePrice ?? 0,
                        Quantity = cpv.Quantity
                    }).ToList();
                response.ChildCombos = null;
                return Result<ComboDetailResponse>.Success(response);
            }
        }

        public async Task<Result<ComboResponse>> CreateComboAsync(CreateComboRequest request)
        {
            // Validate SalePrice <= BasePrice
            if (request.SalePrice > request.BasePrice)
            {
                return Result<ComboResponse>.Failure(
                    "Sale price cannot be greater than base price.", 400);
            }

            // Validate slug uniqueness
            if (await _comboRepository.SlugExistsAsync(request.Slug))
            {
                return Result<ComboResponse>.Failure(
                    "A combo with this slug already exists.", 400);
            }

            // Parent combo cannot have product items
            if (request.ComboParentId == null && request.Items != null && request.Items.Any())
            {
                return Result<ComboResponse>.Failure(
                    "Parent combo cannot have product items. Add items to child combos instead.", 400);
            }

            // Child combo validations
            if (request.ComboParentId != null)
            {
                var parent = await _comboRepository.GetComboByIdAsync(request.ComboParentId.Value);
                if (parent == null)
                {
                    return Result<ComboResponse>.Failure("Parent combo not found.", 404);
                }

                if (parent.ComboParentId != null)
                {
                    return Result<ComboResponse>.Failure(
                        "Cannot nest a child combo under another child combo.", 400);
                }

                if (request.Items == null || !request.Items.Any())
                {
                    return Result<ComboResponse>.Failure(
                        "Child combo must have at least one product item.", 400);
                }

                // Check for duplicate ProductVariantIds
                var duplicateIds = request.Items
                    .GroupBy(i => i.ProductVariantId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();
                if (duplicateIds.Any())
                {
                    return Result<ComboResponse>.Failure(
                        $"Duplicate product variant IDs: {string.Join(", ", duplicateIds)}.", 400);
                }

                // Validate all product variant IDs exist
                foreach (var item in request.Items)
                {
                    var variant = await _variantRepository.GetVariantByIdAsync(item.ProductVariantId);
                    if (variant == null)
                    {
                        return Result<ComboResponse>.Failure(
                            $"Product variant '{item.ProductVariantId}' not found.", 404);
                    }
                }
            }

            // Map and create
            var combo = _mapper.Map<Combo>(request);
            combo.Id = Guid.NewGuid();
            combo.IsActive = true;
            combo.CreatedAt = DateTime.UtcNow;
            combo.AverageRating = 0;

            var res = await _comboRepository.CreateAsync(combo);
            if (res < 0)
            {
                return Result<ComboResponse>.Failure("Failed to create combo.", 400);
            }

            // Create ComboProductVariant entries for child combo
            if (request.ComboParentId != null && request.Items != null && request.Items.Any())
            {
                var comboItems = request.Items.Select(i => new ComboProductVariant
                {
                    Id = Guid.NewGuid(),
                    ComboId = combo.Id,
                    ProductVariantId = i.ProductVariantId,
                    Quantity = i.Quantity
                }).ToList();

                await _comboRepository.AddComboProductVariantsAsync(comboItems);
            }

            return Result<ComboResponse>.Success(MapToComboResponse(combo));
        }

        public async Task<Result<ComboResponse>> UpdateComboInfoAsync(
            Guid id, UpdateComboInfoRequest request)
        {
            var combo = await _comboRepository.GetComboByIdAsync(id);
            if (combo == null)
            {
                return Result<ComboResponse>.Failure("Combo not found.", 404);
            }

            if (request.SalePrice > request.BasePrice)
            {
                return Result<ComboResponse>.Failure(
                    "Sale price cannot be greater than base price.", 400);
            }

            if (await _comboRepository.SlugExistsAsync(request.Slug, id))
            {
                return Result<ComboResponse>.Failure(
                    "A combo with this slug already exists.", 400);
            }

            _mapper.Map(request, combo);

            var res = await _comboRepository.UpdateAsync(combo);
            if (res < 0)
            {
                return Result<ComboResponse>.Failure("Failed to update combo.", 400);
            }

            return Result<ComboResponse>.Success(MapToComboResponse(combo));
        }

        public async Task<Result<bool>> UpdateComboProductsAsync(
            Guid id, UpdateComboProductsRequest request)
        {
            var combo = await _comboRepository.GetComboByIdAsync(id);
            if (combo == null)
            {
                return Result<bool>.Failure("Combo not found.", 404);
            }

            // Only child combos can have product items
            if (combo.ComboParentId == null)
            {
                return Result<bool>.Failure(
                    "Cannot update product list for a parent combo. Parent combos do not have product items.", 400);
            }

            // Check for duplicate ProductVariantIds
            var duplicateIds = request.Items
                .GroupBy(i => i.ProductVariantId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            if (duplicateIds.Any())
            {
                return Result<bool>.Failure(
                    $"Duplicate product variant IDs: {string.Join(", ", duplicateIds)}.", 400);
            }

            // Validate all product variant IDs exist
            foreach (var item in request.Items)
            {
                var variant = await _variantRepository.GetVariantByIdAsync(item.ProductVariantId);
                if (variant == null)
                {
                    return Result<bool>.Failure(
                        $"Product variant '{item.ProductVariantId}' not found.", 404);
                }
            }

            // Remove existing and replace with new items
            await _comboRepository.RemoveComboProductVariantsAsync(id);

            var comboItems = request.Items.Select(i => new ComboProductVariant
            {
                Id = Guid.NewGuid(),
                ComboId = id,
                ProductVariantId = i.ProductVariantId,
                Quantity = i.Quantity
            }).ToList();

            await _comboRepository.AddComboProductVariantsAsync(comboItems);

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeleteComboAsync(Guid id)
        {
            var combo = await _comboRepository.GetComboByIdAsync(id);
            if (combo == null)
            {
                return Result<bool>.Failure("Combo not found.", 404);
            }

            combo.IsActive = false;
            var res = await _comboRepository.UpdateAsync(combo);
            if (res < 0)
            {
                return Result<bool>.Failure("Failed to delete combo.", 400);
            }

            // Cascade soft delete: if parent combo, also soft delete all children
            if (combo.ComboParentId == null)
            {
                var parentWithChildren = await _comboRepository.GetComboWithChildrenAsync(id);
                if (parentWithChildren?.ComboChildrens != null)
                {
                    foreach (var child in parentWithChildren.ComboChildrens)
                    {
                        child.IsActive = false;
                        await _comboRepository.UpdateAsync(child);
                    }
                }
            }

            return Result<bool>.Success(true);
        }

        private static ComboResponse MapToComboResponse(Combo c)
        {
            return new ComboResponse
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                AgeGroup = c.AgeGroup,
                Size = c.Size,
                Color = c.Color,
                BasePrice = c.BasePrice,
                SalePrice = c.SalePrice,
                ImageUrl = c.ImageUrl,
                AverageRating = c.AverageRating,
                ComboParentId = c.ComboParentId
            };
        }

        private static ComboDetailResponse MapToDetailResponse(Combo c)
        {
            return new ComboDetailResponse
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                AgeGroup = c.AgeGroup,
                Size = c.Size,
                Color = c.Color,
                BasePrice = c.BasePrice,
                SalePrice = c.SalePrice,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                AverageRating = c.AverageRating,
                CreatedAt = c.CreatedAt,
                ComboParentId = c.ComboParentId
            };
        }
    }
}
