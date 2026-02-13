using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IAddressService
    {
        Task<Result<PaginatedList<AddressResponse>>> GetAllAsync(string userId, int pageNumber);
        Task<Result<AddressResponse>> GetByIdAsync(string userId, string addressId);
        Task<Result> CreateAsync(Address address);
        Task<Result> UpdateAsync(string userId, string addressId, AddressUpdateRequest addressRequest);
        Task<Result> RemoveAsync(string userId, string addressId);
    }
}
