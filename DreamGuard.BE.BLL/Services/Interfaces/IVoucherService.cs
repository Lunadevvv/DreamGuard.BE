using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IVoucherService
    {
        Task<Result<PaginatedList<VoucherResponse>>> GetAllByAdminAsync(int pageNumber);
        Task<Result<PaginatedList<VoucherResponse>>> GetAllForUserAsync(Guid userId, int pageNumber);
        Task<Result<VoucherResponse>> GetByIdAsync(Guid userId, Guid voucherId);
        Task<Result<VoucherResponse>> GetByIdAsync(Guid voucherId);
        Task<Result> CreateAsync(Voucher voucher);
        Task<Result> UpdateAsync(Guid voucherId, VoucherUpdateRequest voucherRequest);
        Task<Result> ToggleActiveAsync(Guid voucherId);
        Task<Result> ClaimVoucherAsync(Guid userId, string code);
    }
}
