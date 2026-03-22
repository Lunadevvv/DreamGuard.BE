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
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(DreamGuardContext context) : base(context)
        {
        }

        public async Task<Customer?> GetByUserIdAsync(Guid userId)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == userId);
        }

        public async Task<Customer?> GetByUserIdWithUserAsync(Guid userId)
        {
            return await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CustomerId == userId);
        }

        public async Task<PaginatedList<Customer>> GetPaginatedListAsync(int pageNumber, int pageSize, string searchName)
        {
            var query = _context.Customers.Include(c => c.User).OrderByDescending(c => c.User.CreatedAt).AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
            {
                query = query.Where(c => c.FullName.Contains(searchName));
            }

            return await PaginatedList<Customer>.CreateAsync(query, pageNumber, pageSize);
        }
    }
}
