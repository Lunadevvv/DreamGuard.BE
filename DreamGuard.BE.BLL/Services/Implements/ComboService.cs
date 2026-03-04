using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
    public class ComboService : IComboService
    {
        private readonly IComboRepository _comboRepository;
        private readonly IProductVariantRepository _variantRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ComboService(
            IComboRepository comboRepository,
            IProductVariantRepository variantRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _comboRepository = comboRepository;
            _variantRepository = variantRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedList<ComboResponse>>> GetAllCombosAsync(
            int pageNumber, decimal? maxPrice, int? maxAgeGroup, string? color)
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

        public async Task<Result<PaginatedList<ComboAdminResponse>>> GetAllCombosForAdminAsync(int pageNumber, string? name, ProductStatus? status)
        {
            var combos = await _comboRepository.GetAllCombosForAdminAsync(pageNumber, name, status);

            if (combos == null || !combos.Items.Any())
            {
                return Result<PaginatedList<ComboAdminResponse>>.Failure(
                    "No combos found.", 404);
            }

            var responses = combos.Items.Select(
                c => new ComboAdminResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    BasePrice = c.BasePrice,
                    SalePrice = c.SalePrice,
                    ImageUrl = c.ImageUrl,
                    AverageRating = c.AverageRating,
                    Status = c.Status,
                    ChildCombos = c.ComboChildrens?.Select(ch => new ComboChildAdminResponse
                    {
                        Id = ch.Id,
                        Name = ch.Name,
                        Slug = ch.Slug,
                        Color = ch.Color,
                        Size = ch.Size,
                        BasePrice = ch.BasePrice,
                        SalePrice = ch.SalePrice,
                        Status = ch.Status
                    }).ToList()
                }
            ).ToList();

            var paginatedResponse = new PaginatedList<ComboAdminResponse>(
                responses, combos.TotalCount, combos.PageNumber, combos.PageSize);

            return Result<PaginatedList<ComboAdminResponse>>.Success(paginatedResponse);
        }

        public async Task<Result<ComboDetailResponse>> GetComboByIdAsync(Guid id, string? size, string? color)
        {
            var combo = await _comboRepository.GetComboByIdAsync(id);
            if (combo == null)
            {
                return Result<ComboDetailResponse>.Failure("Combo not found.", 404);
            }

            if (combo.ComboParentId == null)
            {
                // Parent combo: load children only
                var parentCombo = await _comboRepository.GetComboWithChildrenAsync(id, size, color);
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
                        AverageRating = ch.AverageRating,
                        Stock = CalculateComboStock(ch.ComboProductVariants)
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
                response.Stock = CalculateComboStock(childCombo!.ComboProductVariants);
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
            combo.Status = ProductStatus.Draft;
            combo.CreatedAt = DateTime.UtcNow;
            combo.AverageRating = 0;

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var res = await _comboRepository.CreateAsync(combo);
                if (res < 0)
                {
                    await transaction.RollbackAsync();
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

                // Auto-recalculate parent prices after child creation
                if (request.ComboParentId != null)
                {
                    var allChildren = await _comboRepository.GetAllChildrenOfParentAsync(request.ComboParentId.Value);
                    if (allChildren.Any())
                    {
                        var parent = await _comboRepository.GetComboByIdAsync(request.ComboParentId.Value);
                        parent!.BasePrice = allChildren.Min(c => c.BasePrice);
                        parent.SalePrice = allChildren.Min(c => c.SalePrice);
                        await _comboRepository.UpdateAsync(parent);
                    }
                }

                await transaction.CommitAsync();
                return Result<ComboResponse>.Success(MapToComboResponse(combo));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Result<ComboResponse>.Failure("Failed to create combo with products.", 500);
            }
        }

        public async Task<Result<ComboResponse>> UpdateComboInfoAsync(
            Guid id, UpdateComboInfoRequest request)
        {
            var combo = await _comboRepository.GetComboWithChildrenAsync(id, "", "");
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

            // Auto-recalculate parent prices when a child combo's price is updated
            if (combo.ComboParentId != null)
            {
                var allChildren = await _comboRepository.GetAllChildrenOfParentAsync(combo.ComboParentId.Value);
                if (allChildren.Any())
                {
                    var parent = await _comboRepository.GetComboByIdAsync(combo.ComboParentId.Value);
                    parent!.BasePrice = allChildren.Min(c => c.BasePrice);
                    parent.SalePrice = allChildren.Min(c => c.SalePrice);
                    await _comboRepository.UpdateAsync(parent);
                }
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
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _comboRepository.RemoveComboProductVariantsAsync(id);

                var comboItems = request.Items.Select(i => new ComboProductVariant
                {
                    Id = Guid.NewGuid(),
                    ComboId = id,
                    ProductVariantId = i.ProductVariantId,
                    Quantity = i.Quantity
                }).ToList();

                await _comboRepository.AddComboProductVariantsAsync(comboItems);

                await transaction.CommitAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Result<bool>.Failure("Failed to update combo products.", 500);
            }
        }

        public async Task<Result<bool>> UpdateComboStatusAsync(Guid id, ProductStatus status)
        {
            var combo = await _comboRepository.GetComboByIdAsync(id);
            if (combo == null)
            {
                return Result<bool>.Failure("Combo not found.", 404);
            }

            // Validate: parent combo can only be Published if it has child combos with at least one Published
            if (status == ProductStatus.Published && combo.ComboParentId == null)
            {
                var allChildren = await _comboRepository.GetAllChildrenOfParentAsync(id);
                if (!allChildren.Any())
                {
                    return Result<bool>.Failure(
                        "Cannot publish a parent combo that has no child combos.", 400);
                }
                if (!allChildren.Any(c => c.Status == ProductStatus.Published))
                {
                    return Result<bool>.Failure(
                        "Cannot publish a parent combo without at least one published child combo.", 400);
                }
            }

            combo.Status = status;
            var res = await _comboRepository.UpdateAsync(combo);
            if (res < 0)
            {
                return Result<bool>.Failure("Failed to update combo status.", 400);
            }

            // Cascade: when parent combo is Hidden, hide ALL child combos (regardless of current status)
            if (status == ProductStatus.Hidden && combo.ComboParentId == null)
            {
                var allChildren = await _comboRepository.GetAllChildrenOfParentAsync(id);
                foreach (var child in allChildren)
                {
                    child.Status = ProductStatus.Hidden;
                    await _comboRepository.UpdateAsync(child);
                }
            }

            // Auto-hide parent when all children become Hidden
            if (status == ProductStatus.Hidden && combo.ComboParentId != null)
            {
                var allSiblings = await _comboRepository.GetAllChildrenOfParentAsync(combo.ComboParentId.Value);
                if (allSiblings.Any() && allSiblings.All(c => c.Status == ProductStatus.Hidden))
                {
                    var parent = await _comboRepository.GetComboByIdAsync(combo.ComboParentId.Value);
                    parent!.Status = ProductStatus.Hidden;
                    await _comboRepository.UpdateAsync(parent);
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

        private static int CalculateComboStock(List<ComboProductVariant> comboProductVariants)
        {
            if (comboProductVariants == null || !comboProductVariants.Any())
            {
                return 0;
            }

            int minStock = int.MaxValue;

            foreach (var cpv in comboProductVariants)
            {
                if (cpv.Quantity <= 0)
                {
                    continue;
                }

                int inventoryQuantity = cpv.ProductVariant?.Inventory?.Quantity ?? 0;
                int possibleSets = inventoryQuantity / cpv.Quantity;
                minStock = Math.Min(minStock, possibleSets);
            }

            return minStock == int.MaxValue ? 0 : minStock;
        }
    }
}
