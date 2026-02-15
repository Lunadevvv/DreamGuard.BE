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
    public class UserProfileService : IUserProfileService
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly IOtpService _otpService;
        private readonly IOtpRepository _otpRepository;

        public UserProfileService(UserManager<User> userManager, IMapper mapper, IOtpService otpService, IOtpRepository otpRepository)
        {
            _mapper = mapper;
            _userManager = userManager;
            _otpService = otpService;
            _otpRepository = otpRepository;
        }
        public async Task<Result<UserProfileResponse>> GetByIdAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return Result<UserProfileResponse>.Failure("UserProfile not found", 404);
            }
            var UserProfileResponse = _mapper.Map<UserProfileResponse>(user);
            return Result<UserProfileResponse>.Success(UserProfileResponse);

        }

        public async Task<Result> UpdateAsync(Guid userId, UserProfileUpdateRequest userProfileUpdateRequest)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return Result.Failure("UserProfile not found", 404);
            }
            _mapper.Map(userProfileUpdateRequest, user);
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return Result.Failure($"{result.Errors}", 400);
            }
            return Result.Success($"{result}");
        }
        public async Task<Result> ChangePhoneNumberRequestAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return Result.Failure("User not found", 404);
            }
            var result = await _otpService.GenerateAndSendOtpAsync(user.PhoneNumber, user.Email);
            if (!result.Succeeded)
            {
                return Result.Failure($"{result.Error}", 400);
            }
            return Result.Success($"{result.Message}");
        }
        public async Task<Result> ChangePhoneNumberAsync(Guid userId, ChangePhoneNumberRequest changePhoneNumberRequest)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return Result.Failure("User not found", 404);
            }
            var otpResult = await _otpService.VerifyOtpAsync(user.PhoneNumber, user.Email, changePhoneNumberRequest.OtpCode);
            if (!otpResult.Succeeded)
            {
                return Result.Failure($"{otpResult.Error}", 400);
            }
            user.PhoneNumber = changePhoneNumberRequest.PhoneNumber;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return Result.Failure($"{result.Errors}", 400);
            }
            return Result.Success($"PhoneNumber changed successfully");
        }
    }
}
