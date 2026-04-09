using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class UserVoucherService : IUserVoucherService
    {
        private readonly IMapper _mapper;
        private readonly IUserVoucherRepository _userVoucherRepository;
        public UserVoucherService( IMapper mapper, IUserVoucherRepository userVoucherRepository)
        {
            _mapper = mapper;
            _userVoucherRepository = userVoucherRepository;
        }

        public async Task<Result<PaginatedList<UserVoucherResponse>>> GetAllByUserAsync(Guid userId, int pageNumber, bool? isUsed = null)
        {
            var result = await _userVoucherRepository.GetAllByUserAsync(userId, pageNumber, isUsed);
            if (result == null)
            {
                return Result<PaginatedList<UserVoucherResponse>>.Failure("Failed to retrieve user vouchers.", 400);
            }

            var response = result.Items.Select(uv => MapToResponse(uv)).ToList();

            return Result<PaginatedList<UserVoucherResponse>>.Success(new PaginatedList<UserVoucherResponse>(response, result.TotalCount, result.PageNumber, result.PageSize));
        }

        public UserVoucherResponse MapToResponse(UserVoucher userVoucher)
        {
            return _mapper.Map<UserVoucherResponse>(userVoucher);
        }
    }
}