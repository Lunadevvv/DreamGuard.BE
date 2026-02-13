using AutoMapper;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Implements;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class BabyProfileService : IBabyProfileService
    {
        private readonly IBabyProfileRepository _repo;
        private readonly IMapper _mapper;

        public BabyProfileService(IBabyProfileRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }
        public async Task<Result<PaginatedList<BabyProfileResponse>>> GetAllAsync(string userId,int pageNumber)
        {
            var babies = await  _repo.GetAllAsync(userId, pageNumber);
            if (babies == null || babies.TotalCount == 0)
            {
                return Result<PaginatedList<BabyProfileResponse>>.Failure("No baby profiles found", 404);
            }
            var babyProfileResponse = _mapper.Map<List<BabyProfileResponse>>(babies.Items);
            var paginatedResult = new PaginatedList<BabyProfileResponse>(babyProfileResponse, babies.TotalCount, babies.PageNumber, babies.PageSize);
            return Result<PaginatedList<BabyProfileResponse>>.Success(paginatedResult);
        }

        public async Task<Result<BabyProfileResponse>> GetByIdAsync(string userId, string babyId)
        {
            var baby = await _repo.GetByIdAsync(userId, babyId);
            if (baby == null)
            {
                return Result<BabyProfileResponse>.Failure("Baby profile not found", 404);
            }
            var babyProfileResponse = _mapper.Map<BabyProfileResponse>(baby);
            return Result<BabyProfileResponse>.Success(babyProfileResponse);

        }
        public async Task<Result> CreateAsync(BabyProfile babyProfile)
        {
            var result = await _repo.CreateAsync(babyProfile);
            return Result.Success($"{result}");
        }
        public async Task<Result> UpdateAsync(string userId, string babyId, BabyProfileUpdateRequest babyProfileRequest)
        {
            var existingBabyProfile = await _repo.GetByIdAsync(userId, babyId);
            if (existingBabyProfile == null)
            {
                return Result.Failure("Baby profile not found", 404);
            }
            _mapper.Map(babyProfileRequest, existingBabyProfile);
            var result = await _repo.UpdateAsync(existingBabyProfile);
            return Result.Success($"{result}");
        }
        public async Task<Result> RemoveAsync(string userId, string babyId)
        {
            var babyProfile = await _repo.GetByIdAsync(userId, babyId);
            if (babyProfile == null)
            {
                return Result.Failure("Baby profile not found", 404);
            }
            var result = await _repo.RemoveAsync(babyProfile);
            return Result.Success($"{result}");
        }
    }
}
