using AutoMapper;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Implements;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class VoucherService : IVoucherService
    {
        private readonly IVoucherRepository _repo;
        private readonly IUserVoucherRepository _userVoucherRepo;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public VoucherService(IVoucherRepository repo, IMapper mapper, UserManager<User> userManager, IUserVoucherRepository userVoucherRepository )
        {
            _mapper = mapper;
            _repo = repo;
            _userManager = userManager;
            _userVoucherRepo = userVoucherRepository;
        }
        public async Task<Result<PaginatedList<VoucherResponse>>> GetAllAsync(Guid userId, int pageNumber)
        {
            var vouchers = await  _repo.GetAllAsync(userId, pageNumber);
            if (vouchers == null || vouchers.TotalCount == 0)
            {
                return Result<PaginatedList<VoucherResponse>>.Failure("No vouchers found", 404);
            }
            var VoucherResponse = _mapper.Map<List<VoucherResponse>>(vouchers.Items);
            var paginatedResult = new PaginatedList<VoucherResponse>(VoucherResponse, vouchers.TotalCount, vouchers.PageNumber, vouchers.PageSize);
            return Result<PaginatedList<VoucherResponse>>.Success(paginatedResult);
        }

        public async Task<Result<VoucherResponse>> GetByIdAsync(Guid userId, Guid voucherId)
        {
            var voucher = await _repo.GetByIdAsync(userId, voucherId);
            if (voucher == null)
            {
                return Result<VoucherResponse>.Failure("Voucher not found", 404);
            }
            var VoucherResponse = _mapper.Map<VoucherResponse>(voucher);
            return Result<VoucherResponse>.Success(VoucherResponse);
        }
        public async Task<Result<VoucherResponse>> GetByIdAsync(Guid voucherId)
        {
            var voucher = await _repo.GetByIdAsync(voucherId);
            if (voucher == null)
            {
                return Result<VoucherResponse>.Failure("Voucher not found", 404);
            }
            var VoucherResponse = _mapper.Map<VoucherResponse>(voucher);
            return Result<VoucherResponse>.Success(VoucherResponse);
        }

        public async Task<Result> CreateAsync(Voucher voucher)
        {
            if(voucher.StartDate >= voucher.EndDate)
            {
                return Result.Failure("Start date must be before end date", 400);
            }
            var result = await _repo.CreateAsync(voucher);
            return Result.Success($"{result}");
        }
        public async Task<Result> UpdateAsync(Guid voucherId, VoucherUpdateRequest voucherRequest)
        {
            if (voucherRequest.StartDate >= voucherRequest.EndDate)
            {
                return Result.Failure("Start date must be before end date", 400);
            }
            var existingVoucher = await _repo.GetByIdAsync(voucherId);
            if (existingVoucher == null)
            {
                return Result.Failure("Voucher  not found", 404);
            }
            _mapper.Map(voucherRequest, existingVoucher);
            var result = await _repo.UpdateAsync(existingVoucher);
            return Result.Success($"{result}");
        }
        public async Task<Result> ToggleActiveAsync(Guid voucherId)
        {
            var voucher = await _repo.GetByIdAsync(voucherId);
            if (voucher == null)
            {
                return Result.Failure("Voucher  not found", 404);
            }
            voucher.IsActive = !voucher.IsActive;
            var result = await _repo.UpdateAsync(voucher);
            return Result.Success($"{result}");
        }

        public async Task<Result<PaginatedList<VoucherResponse>>> GetAllByAdminAsync(int pageNumber)
        {
            var vouchers = await _repo.GetAllByAdminAsync(pageNumber);
            if (vouchers == null || vouchers.TotalCount == 0)
            {
                return Result<PaginatedList<VoucherResponse>>.Failure("No vouchers found", 404);
            }
            var VoucherResponse = _mapper.Map<List<VoucherResponse>>(vouchers.Items);
            var paginatedResult = new PaginatedList<VoucherResponse>(VoucherResponse, vouchers.TotalCount, vouchers.PageNumber, vouchers.PageSize);
            return Result<PaginatedList<VoucherResponse>>.Success(paginatedResult);
        }

        public async Task<Result> ClaimVoucherAsync(Guid userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return Result.Failure("User not found", 404);
            }
            var voucher = await _repo.GetByCodeAsync(code);
            if (voucher == null)
            {
                return Result.Failure("Voucher not found", 404);
            }
            if (!voucher.IsActive)
            {
                return Result.Failure("Voucher is not active", 400);
            }
            if (voucher.StartDate > DateTime.UtcNow)
            {
                return Result.Failure("Voucher is not yet available", 400);
            }
            if (voucher.EndDate < DateTime.UtcNow)
            {
                return Result.Failure("Voucher has expired", 400);
            }
            var alreadyClaimed = await _userVoucherRepo.ExistsAsync(userId, voucher.VoucherId);
            if (alreadyClaimed)
            {
                return Result.Failure("You have already claimed this voucher", 400);
            }
            var userVoucher = new UserVoucher
            {
                UserId = userId,
                VoucherId = voucher.VoucherId,
                IsUsed = false,
                ExpiredAt = voucher.EndDate

            };
            var result = await _userVoucherRepo.CreateAsync(userVoucher);
            return Result.Success($"{result}");
        }
    }
}
