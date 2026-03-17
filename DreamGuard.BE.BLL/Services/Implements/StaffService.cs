using AutoMapper;
using CloudinaryDotNet.Actions;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class StaffService : IStaffService
    {
        private readonly IStaffRepository _repo;
        private readonly IMapper _mapper;
        
      
        public StaffService(IStaffRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<Result> CreateAsync(StaffCreateRequest staffCreateRequest)
        {
            var addStaff = _mapper.Map<StaffCreateRequest, Staff>(staffCreateRequest);
            var result = await _repo.CreateAsync(addStaff);
            return Result.Success($"{result}");
        }

        public async Task<Result<PaginatedList<StaffResponse>>> GetAllAsync(int pageNumber, int pageSize)
        {
            var staffs = await _repo.GetAllByAdminAsync(pageNumber, pageSize);
            var staffResponse = _mapper.Map<List<StaffResponse>>(staffs.Items);
            var paginatedResult = new PaginatedList<StaffResponse>(staffResponse, staffs.TotalCount, staffs.PageNumber, staffs.PageSize);
            return Result<PaginatedList<StaffResponse>>.Success(paginatedResult);
        }

        public async Task<Result<StaffResponse>> GetByIdAsync(Guid staffId)
        {
            var result = await _repo.GetByIdAsync(staffId);
            if (result == null)
            {
                return Result<StaffResponse>.Failure("Staff not found.", 404);
            }
            var staffResponse = _mapper.Map<Staff, StaffResponse>(result);
            return Result<StaffResponse>.Success(staffResponse);
        }

        public async Task<Result> UpdateAsync(Guid staffId,StaffUpdateRequest staffUpdateRequest)
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
    }
}