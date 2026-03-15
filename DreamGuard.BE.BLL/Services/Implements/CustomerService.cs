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
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repo;
        private readonly IMapper _mapper;
        
      
        public CustomerService(ICustomerRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<Result> CreateAsync(CustomerCreateRequest customerCreateRequest)
        {
            var addCustomer = _mapper.Map<CustomerCreateRequest, Customer>(customerCreateRequest);
            var result = await _repo.CreateAsync(addCustomer);
            return Result.Success($"{result}");
        }

        public async Task<Result<CustomerResponse>> GetByIdAsync(Guid customerId)
        {
            var result = await _repo.GetByIdAsync(customerId);
            if (result == null)
            {
                return Result<CustomerResponse>.Failure("Customer not found.", 404);
            }
            var customerResponse = _mapper.Map<Customer, CustomerResponse>(result);
            return Result<CustomerResponse>.Success(customerResponse);
        }

        public async Task<Result> UpdateAsync(Guid customerId,CustomerUpdateRequest customerUpdateRequest)
        {
            var existingCustomer = await _repo.GetByIdAsync(customerId);
            _mapper.Map(customerUpdateRequest, existingCustomer);
            var result = await _repo.UpdateAsync(existingCustomer);
            return Result.Success($"{result}");
        }
    }
}