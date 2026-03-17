using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.DbContext;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Repositories.Implements
{
    public class AddressRepository : GenericRepository<Address>, IAddressRepository
    {
        public AddressRepository(DreamGuardContext context) : base(context) { }

        public async Task<Address> GetByIdAsync(Guid customerId, Guid addressId)
        {
            try
            {
                return await _context.Addresses.FirstOrDefaultAsync(ad => ad.CustomerId == customerId && ad.AddressId == addressId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving addresse for customer {customerId}: {ex.Message}");
            }
        }

        public async Task<PaginatedList<Address>> GetAllAsync(Guid customerId, int pageNumber)
        {
            try
            {
                var query = _context.Addresses.Where(ad => ad.CustomerId == customerId);
                return await PaginatedList<Address>.CreateAsync(query, pageNumber, 4);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving addresses for customer {customerId}: {ex.Message}");
            }
        }
    }
}
