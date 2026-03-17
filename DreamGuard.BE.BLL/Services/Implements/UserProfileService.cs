using AutoMapper;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Implements;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class UserProfileService : IUserProfileService
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly IOtpService _otpService;
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UserProfileService(UserManager<User> userManager, IMapper mapper, IOtpService otpService, ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _userManager = userManager;
            _otpService = otpService;
            _unitOfWork = unitOfWork;
            _customerRepository = customerRepository;
        }
        public async Task<Result<UserProfileResponse>> GetByIdAsync(Guid userId)
        {
            var customer = await _customerRepository.GetByUserIdWithUserAsync(userId);

            if (customer == null)
                return Result<UserProfileResponse>.Failure("Profile not found", 404);

            var response = _mapper.Map<UserProfileResponse>(customer);
            return Result<UserProfileResponse>.Success(response);
        }

        public async Task<Result> UpdateAsync(Guid userId, UserProfileUpdateRequest request)
        {
            var customer = await _customerRepository.GetByUserIdWithUserAsync(userId);

            if (customer == null)
                return Result.Failure("Profile not found", 404);

            if (request.Gender != DAL.Constants.Gender.Male && request.Gender != DAL.Constants.Gender.Female)
                return Result.Failure("Gender must be 'Male' or 'Female'", 400);

            _mapper.Map(request, customer);

            if (!string.IsNullOrEmpty(request.Email) && customer.User.Email != request.Email)
            {
                customer.User.Email = request.Email;
                await _userManager.UpdateAsync(customer.User);
            }

            await _customerRepository.UpdateAsync(customer);
            return Result.Success("Profile updated successfully");
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
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var otpResult = await _otpService.VerifyOtpAsync(user.PhoneNumber, user.Email, changePhoneNumberRequest.OtpCode);
                if (!otpResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure($"{otpResult.Error}", 400);
                }

                var existingUser = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == changePhoneNumberRequest.PhoneNumber && u.Id != userId);

                if (existingUser != null)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure("Phone number is already in use by another account", 400);
                }

                user.PhoneNumber = changePhoneNumberRequest.PhoneNumber;
                user.UserName = changePhoneNumberRequest.PhoneNumber;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)), 400);
                }
                
                await transaction.CommitAsync();
                return Result.Success($"PhoneNumber changed successfully");
            }catch
            {
                await transaction.RollbackAsync();
                throw;
            }
            
        }
    }
}
