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

        public async Task<Result> AddCoinAsync(Customer customer, int amount)
        {
            customer.MemberCoin += amount;
            var result = await _repo.UpdateAsync(customer);
            return result > 0 ? Result.Success("Success add coin") : Result.Failure("Failed to add coins to the customer.", 500);
        }

        public async Task<Result<CustomerResponse>> GetByIdAsync(Guid customerId)
        {
            var result = await _repo.GetByUserIdWithUserAsync(customerId);
            if (result == null)
            {
                return Result<CustomerResponse>.Failure("Customer not found.", 404);
            }
            var customerResponse = _mapper.Map<Customer, CustomerResponse>(result);
            return Result<CustomerResponse>.Success(customerResponse);
        }

        public async Task<Result<PaginatedList<CustomerResponse>>> GetPaginatedListAsync(int pageNumber, int pageSize, string searchName)
        {
            var result = await _repo.GetPaginatedListAsync(pageNumber, pageSize, searchName);
            var customerResponses = _mapper.Map<PaginatedList<Customer>, PaginatedList<CustomerResponse>>(result);
            return Result<PaginatedList<CustomerResponse>>.Success(customerResponses);
        }
    }
}