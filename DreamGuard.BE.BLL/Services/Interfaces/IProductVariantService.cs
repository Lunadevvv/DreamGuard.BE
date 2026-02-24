using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IProductVariantService
    {
        Task<Result<List<ProductVariantResponse>>> GetVariantsByProductIdAsync(Guid productId, string? size, string? color);
        Task<Result<ProductVariantResponse>> GetVariantByIdAsync(Guid id);
        Task<Result<ProductVariantResponse>> CreateVariantAsync(CreateProductVariantRequest request);
        Task<Result<ProductVariantResponse>> UpdateVariantAsync(Guid id, UpdateProductVariantRequest request);
        Task<Result<bool>> UpdateVariantStatusAsync(Guid id, bool isActive);
        Task<Result<bool>> DeleteVariantAsync(Guid id);
    }
}
