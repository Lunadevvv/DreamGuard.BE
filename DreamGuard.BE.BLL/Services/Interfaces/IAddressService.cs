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
        Task<Result<PaginatedList<AddressResponse>>> GetAllAsync(Guid userId, int pageNumber);
        Task<Result<AddressResponse>> GetByIdAsync(Guid userId, Guid addressId);
        Task<Result> CreateAsync(Guid userId, Address address);
        Task<Result> UpdateAsync(Guid userId, Guid addressId, AddressUpdateRequest addressRequest);
        Task<Result> RemoveAsync(Guid userId, Guid addressId);
    }
}
