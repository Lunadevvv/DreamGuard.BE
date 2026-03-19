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

        public async Task<bool> MarkAsUsedAsync(Guid userVoucherId)
        {
            var result = await _context.UserVouchers.Where(v => v.UserVoucherId == userVoucherId && !v.IsUsed)
                 .ExecuteUpdateAsync(s => s
                     .SetProperty(v => v.IsUsed, true)
                     .SetProperty(v => v.UsedAt, DateTime.UtcNow));
            return result > 0;
        }
        public async Task<UserVoucher?> GetByIdAsync(Guid userVoucherId)
        {
            return await _context.UserVouchers
                .Include(uv => uv.Voucher)
                .FirstOrDefaultAsync(uv => uv.UserVoucherId == userVoucherId);
        }
    }
}
