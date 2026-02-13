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
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _repo;
        private readonly IMapper _mapper;

        public AddressService(IAddressRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }
        public async Task<Result<PaginatedList<AddressResponse>>> GetAllAsync(string userId, int pageNumber)
        {
            var addresses = await  _repo.GetAllAsync(userId, pageNumber);
            if (addresses == null || addresses.TotalCount == 0)
            {
                return Result<PaginatedList<AddressResponse>>.Failure("No Addresses found", 404);
            }
            var addressResponse = _mapper.Map<List<AddressResponse>>(addresses.Items);
            var paginatedResult = new PaginatedList<AddressResponse>(addressResponse, addresses.TotalCount, addresses.PageNumber, addresses.PageSize);
            return Result<PaginatedList<AddressResponse>>.Success(paginatedResult);
        }

        public async Task<Result<AddressResponse>> GetByIdAsync(string userId, string addressId)
        {
            var address = await _repo.GetByIdAsync(userId, addressId);
            if (address == null)
            {
                return Result<AddressResponse>.Failure("address not found", 404);
            }
            var AddressResponse = _mapper.Map<AddressResponse>(address);
            return Result<AddressResponse>.Success(AddressResponse);

        }
        public async Task<Result> CreateAsync(Address Address)
        {
            var result = await _repo.CreateAsync(Address);
            return Result.Success($"{result}");
        }
        public async Task<Result> UpdateAsync(string userId, string addressId, AddressUpdateRequest addressRequest)
        {
            var existingAddress = await _repo.GetByIdAsync(userId, addressId);
            if (existingAddress == null)
            {
                return Result.Failure("address  not found", 404);
            }
            _mapper.Map(addressRequest, existingAddress);
            var result = await _repo.UpdateAsync(existingAddress);
            return Result.Success($"{result}");
        }
        public async Task<Result> RemoveAsync(string userId, string addressId)
        {
            var Address = await _repo.GetByIdAsync(userId, addressId);
            if (Address == null)
            {
                return Result.Failure("address  not found", 404);
            }
            var result = await _repo.RemoveAsync(Address);
            return Result.Success($"{result}");
        }
    }
}
