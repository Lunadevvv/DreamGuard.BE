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
        private readonly ICustomerRepository _customerRepository;

        public AddressService(IAddressRepository repo, IMapper mapper, ICustomerRepository customerRepository)
        {
            _mapper = mapper;
            _repo = repo;
            _customerRepository = customerRepository;
        }
        public async Task<Result<PaginatedList<AddressResponse>>> GetAllAsync(Guid userId, int pageNumber)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result<PaginatedList<AddressResponse>>.Failure("Customer profile not found", 404);

            var customerId = customer.CustomerId;

            var addresses = await  _repo.GetAllAsync(customerId, pageNumber);
            if (addresses == null || addresses.TotalCount == 0)
            {
                return Result<PaginatedList<AddressResponse>>.Failure("No Addresses found", 404);
            }
            var addressResponse = _mapper.Map<List<AddressResponse>>(addresses.Items);
            var paginatedResult = new PaginatedList<AddressResponse>(addressResponse, addresses.TotalCount, addresses.PageNumber, addresses.PageSize);
            return Result<PaginatedList<AddressResponse>>.Success(paginatedResult);
        }

        public async Task<Result<AddressResponse>> GetByIdAsync(Guid userId, Guid addressId)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result<AddressResponse>.Failure("Customer profile not found", 404);

            var customerId = customer.CustomerId;

            var address = await _repo.GetByIdAsync(customerId, addressId);
            if (address == null)
            {
                return Result<AddressResponse>.Failure("address not found", 404);
            }
            var AddressResponse = _mapper.Map<AddressResponse>(address);
            return Result<AddressResponse>.Success(AddressResponse);

        }
        public async Task<Result> CreateAsync(Guid userId, Address Address)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result.Failure("Customer profile not found", 404);

            Address.CustomerId = customer.CustomerId;

            var result = await _repo.CreateAsync(Address);
            return Result.Success($"Created new address successfully!");
        }
        public async Task<Result> UpdateAsync(Guid userId, Guid addressId, AddressUpdateRequest addressRequest)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result.Failure("Customer profile not found", 404);

            var customerId = customer.CustomerId;

            var existingAddress = await _repo.GetByIdAsync(customerId, addressId);
            if (existingAddress == null)
            {
                return Result.Failure("address  not found", 404);
            }
            _mapper.Map(addressRequest, existingAddress);
            var result = await _repo.UpdateAsync(existingAddress);
            return Result.Success($"{result}");
        }
        public async Task<Result> RemoveAsync(Guid userId, Guid addressId)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result.Failure("Customer profile not found", 404);

            var customerId = customer.CustomerId;

            var Address = await _repo.GetByIdAsync(customerId, addressId);
            if (Address == null)
            {
                return Result.Failure("address  not found", 404);
            }
            var result = await _repo.RemoveAsync(Address);
            return Result.Success($"{result}");
        }
    }
}
