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
    public class VoucherRepository : GenericRepository<Voucher>, IVoucherRepository
    {
        public VoucherRepository(DreamGuardContext context) : base(context) { }

        public async Task<Voucher> GetByCodeAsync(string code)
        {
            try
            {
                return await _context.Vouchers.FirstOrDefaultAsync(v => v.Code == code && v.IsActive);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving Voucher with code {code}: {ex.Message}");
            }
        }

        public async Task<Voucher> GetByIdAsync(Guid customerId, Guid voucherId)
        {
            try
            {
                return await _context.UserVouchers
                    .Where(uv => uv.CustomerId == customerId && uv.VoucherId == voucherId && uv.Voucher.IsActive)
                    .Select(uv => uv.Voucher)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving Vouchere for customer {customerId}: {ex.Message}");
            }
        }

        public async Task<PaginatedList<Voucher>> GetAllAsync(int pageNumber, List<Guid> claimedVoucherIds)
        {
            try
            {
                var query = _context.Vouchers
                    .Where(uv => uv.IsActive && !claimedVoucherIds.Contains(uv.VoucherId));
                return await PaginatedList<Voucher>.CreateAsync(query, pageNumber, 4);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving Vouchers: {ex.Message}");
            }
        }
        public async Task<PaginatedList<Voucher>> GetAllByAdminAsync(int pageNumber)
        {
            try
            {
                var query = _context.Vouchers;
                return await PaginatedList<Voucher>.CreateAsync(query, pageNumber, 4);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving Vouchers for admin");
            }
        }
    }
}
