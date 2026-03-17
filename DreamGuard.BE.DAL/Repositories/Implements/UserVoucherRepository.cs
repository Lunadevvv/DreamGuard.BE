using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.DbContext;
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
    public class UserVoucherRepository : GenericRepository<UserVoucher>, IUserVoucherRepository
    {
        public UserVoucherRepository(DreamGuardContext context) : base(context) { }

        public async Task<bool> ExistsAsync(Guid customerId, Guid voucherId)
        {
            return await _context.UserVouchers
                .AnyAsync(uv => uv.CustomerId == customerId && uv.VoucherId == voucherId);
        }
    }
}
