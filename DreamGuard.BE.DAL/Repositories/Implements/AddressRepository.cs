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

        public async Task<Address> GetByIdAsync(string userId, string addressId)
        {
            try
            {
                return await _context.Addresses.FirstOrDefaultAsync(ad => ad.UserId == userId && ad.AddressId == addressId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving addresse for user {userId}: {ex.Message}");
            }
        }

        public async Task<PaginatedList<Address>> GetAllAsync(string userId, int pageNumber)
        {
            try
            {
                var query = _context.Addresses.Where(ad => ad.UserId == userId);
                return await PaginatedList<Address>.CreateAsync(query, pageNumber, 4);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving addresses for user {userId}: {ex.Message}");
            }
        }
    }
}
