using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IProductVariantService
    {
        Task<Result<List<ProductVariantResponse>>> GetVariantsByProductIdAsync(Guid productId, string? size, string? color);
        Task<Result<ProductVariantAdminResponse>> GetVariantsByProductIdForAdminAsync(Guid productId);
        Task<Result<ProductVariantResponse>> GetVariantByIdAsync(Guid id);
        Task<Result<ProductVariantResponse>> CreateVariantAsync(CreateProductVariantRequest request);
        Task<Result<ProductVariantResponse>> UpdateVariantAsync(Guid id, UpdateProductVariantRequest request);
        Task<Result<bool>> UpdateVariantStatusAsync(Guid id, ProductStatus status);
        Task<Result<ProductVariantResponse>> CreateVariantWithCustomizeAsync(CreateVariantWithCustomizeRequest request);
        Task<Result> AssignCustomizeTypeAsync(Guid variantId, AssignCustomizeTypeRequest request);
        Task<Result> RemoveCustomizeTypeAsync(Guid variantId, Guid customizeTypeId);
        Task<Result> UpdateCustomizeTypePriceAsync(Guid variantId, Guid customizeTypeId, UpdateCustomizeTypePriceRequest request);
    }
}
