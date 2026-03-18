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
        private readonly ICustomerRepository _customerRepository;

        public BabyProfileService(IBabyProfileRepository repo, IMapper mapper, ICustomerRepository customerRepository)
        {
            _mapper = mapper;
            _repo = repo;
            _customerRepository = customerRepository;
        }
        public async Task<Result<PaginatedList<BabyProfileResponse>>> GetAllAsync(Guid userId,int pageNumber)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result<PaginatedList<BabyProfileResponse>>.Failure("Customer profile not found", 404);

            var customerId = customer.CustomerId;

            var babies = await  _repo.GetAllAsync(customerId, pageNumber);
            if (babies == null || babies.TotalCount == 0)
            {
                return Result<PaginatedList<BabyProfileResponse>>.Failure("No baby profiles found", 404);
            }
            var babyProfileResponse = _mapper.Map<List<BabyProfileResponse>>(babies.Items);
            var paginatedResult = new PaginatedList<BabyProfileResponse>(babyProfileResponse, babies.TotalCount, babies.PageNumber, babies.PageSize);
            return Result<PaginatedList<BabyProfileResponse>>.Success(paginatedResult);
        }

        public async Task<Result<BabyProfileResponse>> GetByIdAsync(Guid userId, Guid babyId)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result<BabyProfileResponse>.Failure("Customer profile not found", 404);

            var customerId = customer.CustomerId;

            var baby = await _repo.GetByIdAsync(customerId, babyId);
            if (baby == null)
            {
                return Result<BabyProfileResponse>.Failure("Baby profile not found", 404);
            }
            var babyProfileResponse = _mapper.Map<BabyProfileResponse>(baby);
            return Result<BabyProfileResponse>.Success(babyProfileResponse);

        }
        public async Task<Result> CreateAsync(Guid userId, BabyProfile babyProfile)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result.Failure("Customer profile not found", 404);

            babyProfile.CustomerId = customer.CustomerId;

            var result = await _repo.CreateAsync(babyProfile);
            return Result.Success($"{result}");
        }
        public async Task<Result> UpdateAsync(Guid userId, Guid babyId, BabyProfileUpdateRequest babyProfileRequest)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result.Failure("Customer profile not found", 404);

            var customerId = customer.CustomerId;

            var existingBabyProfile = await _repo.GetByIdAsync(customerId, babyId);
            if (existingBabyProfile == null)
            {
                return Result.Failure("Baby profile not found", 404);
            }
            _mapper.Map(babyProfileRequest, existingBabyProfile);
            var result = await _repo.UpdateAsync(existingBabyProfile);
            return Result.Success($"{result}");
        }
        public async Task<Result> RemoveAsync(Guid userId, Guid babyId)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result.Failure("Customer profile not found", 404);

            var customerId = customer.CustomerId;

            var babyProfile = await _repo.GetByIdAsync(customerId, babyId);
            if (babyProfile == null)
            {
                return Result.Failure("Baby profile not found", 404);
            }
            var result = await _repo.RemoveAsync(babyProfile);
            return Result.Success($"{result}");
        }
    }
}
