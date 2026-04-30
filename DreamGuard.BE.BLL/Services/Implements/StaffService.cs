using AutoMapper;
using CloudinaryDotNet.Actions;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Implements;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class StaffService : IStaffService
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly IStaffRepository _repo;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        public StaffService(IStaffRepository repo, IMapper mapper, UserManager<User> userManager, IUnitOfWork unitOfWork, IRatingRepository ratingRepository)
        {
            _repo = repo;
            _mapper = mapper;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _ratingRepository = ratingRepository;
        }

        public async Task<Result<PaginatedList<StaffResponse>>> GetAllAsync(int pageNumber, int pageSize)
        {
            var staffs = await _repo.GetAllByAdminAsync(pageNumber, pageSize);
            var staffResponse = _mapper.Map<List<StaffResponse>>(staffs.Items);
            var staffDict = staffs.Items.ToDictionary(sr => sr.StaffId);
            foreach (var response in staffResponse)
            {
                if (response.Position == DAL.Constants.Role.CleaningStaff)
                {
                    var serviceTask = staffDict[response.StaffId].ServiceTasks;
                    response.TaskCount = serviceTask.Count(st => st.Status == ServiceTaskStatus.Pending || st.Status == ServiceTaskStatus.CheckedOut || st.Status == ServiceTaskStatus.CheckedIn || st.Status == ServiceTaskStatus.Processing);
                }else if (response.Position == DAL.Constants.Role.DeliveryStaff)
                {
                    var shippingTask = staffDict[response.StaffId].ShippingTasks;
                    response.TaskCount = shippingTask.Count(o => o.Status == ShippingTaskStatus.Pending || o.Status == ShippingTaskStatus.Arrived || o.Status == ShippingTaskStatus.Delivering || o.Status == ShippingTaskStatus.Returning);
                }
                
            }
            var paginatedResult = new PaginatedList<StaffResponse>(staffResponse, staffs.TotalCount, staffs.PageNumber, staffs.PageSize);
            return Result<PaginatedList<StaffResponse>>.Success(paginatedResult);
        }

        public async Task<Result<StaffResponse>> GetByIdAsync(Guid staffId)
        {
            var result = await _repo.GetByUserIdAsync(staffId);
            if (result == null)
            {
                return Result<StaffResponse>.Failure("Staff not found.", 404);
            }
            var staffResponse = _mapper.Map<Staff, StaffResponse>(result);
            return Result<StaffResponse>.Success(staffResponse);
        }
        public async Task<Result<PaginatedList<RatingResponse>>> GetRatings(Guid staffId, int pageNumber, int pageSize)
        {
            var ratings = await _ratingRepository.GetRatingsByStaffIdAsync(staffId, pageNumber, pageSize);
            var ratingResponses = _mapper.Map<List<RatingResponse>>(ratings.Items);
            var paginatedResult = new PaginatedList<RatingResponse>(ratingResponses, ratings.TotalCount, ratings.PageNumber, ratings.PageSize);
            return Result<PaginatedList<RatingResponse>>.Success(paginatedResult);
        }
        public async Task<Result> UpdateAccountAsync(Guid staffId, StaffAccountUpdateRequest staffUpdateRequest)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == staffId);
            if (user == null)
            {
                return Result.Failure("Staff not found.", 404);
            }

            user.PhoneNumber = staffUpdateRequest.PhoneNumber;
            user.Email = staffUpdateRequest.Email;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await _userManager.ResetPasswordAsync(user, token, staffUpdateRequest.Password!);
            if (!passwordResult.Succeeded)
            {
                return Result.Failure("Failed to update password.", 500);
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return Result.Failure("Failed to update account information.", 500);
            }

            return Result.Success("Account information updated successfully.");
        }

        public async Task<Result> UpdateAsync(Guid staffId, StaffUpdateRequest staffUpdateRequest)
        {
            var existingStaff = await _repo.GetByIdAsync(staffId);
            if (existingStaff == null)
            {
                return Result.Failure("Staff not found.", 404);
            }
            _mapper.Map(staffUpdateRequest, existingStaff);
            var result = await _repo.UpdateAsync(existingStaff);
            return Result.Success($"{result}");
        }

        public async Task<Result> UpdateRoleAsync(Guid staffId, string newRole)
        {
            if(newRole != DAL.Constants.Role.Seller && newRole != DAL.Constants.Role.Manager && newRole != DAL.Constants.Role.CleaningStaff)
            {
                return Result.Failure("Invalid role. Role must be either 'Seller' or 'Manager' or 'CleaningStaff'.", 400);
            }

            var staff = await _repo.GetByUserIdAsync(staffId);

            if (staff == null)
            {
                return Result.Failure("Staff not found.", 404);
            }

            var user = staff.User;
            var roleExists = await _userManager.IsInRoleAsync(user, newRole);
            if (roleExists)
            {
                return Result.Failure("Staff already has the specified role.", 400);
            }

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (!removeResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure("Failed to remove existing roles.", 500);
                }

                var addResult = await _userManager.AddToRoleAsync(user, newRole);

                if (!addResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure("Failed to add new role.", 500);
                }

                //Update staff's position in Staff table
                staff.Position = newRole;
                var res = await _repo.UpdateAsync(staff);

                if (res <= 0)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure("Failed to update staff position.", 500);
                }

                await transaction.CommitAsync();
                return Result.Success("Role updated successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result.Failure($"An error occurred while updating roles: {ex.Message}", 500);
            }
        }
    }
}