using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface IAddressRepository : IGenericRepository<Address>
    {
        Task<PaginatedList<Address>> GetAllAsync(Guid customerId, int pageNumber);
        Task<Address> GetByIdAsync(Guid customerId, Guid addressId);

    }
}
