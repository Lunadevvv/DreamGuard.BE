using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.DAL.ModelExtensions;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IComboService
    {
        Task<Result<PaginatedList<ComboResponse>>> GetAllCombosAsync(
            int pageNumber, double? maxPrice, int? maxAgeGroup, string? color);
        Task<Result<ComboDetailResponse>> GetComboByIdAsync(Guid id, string? size, string? color);
        Task<Result<ComboResponse>> CreateComboAsync(CreateComboRequest request);
        Task<Result<ComboResponse>> UpdateComboInfoAsync(Guid id, UpdateComboInfoRequest request);
        Task<Result<bool>> UpdateComboProductsAsync(Guid id, UpdateComboProductsRequest request);
        Task<Result<bool>> DeleteComboAsync(Guid id);
    }
}
