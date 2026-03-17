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
    public interface IVoucherRepository : IGenericRepository<Voucher>
    {
        Task<Voucher> GetByCodeAsync(string code);
        Task<PaginatedList<Voucher>> GetAllAsync(Guid customerId, int pageNumber);
        Task<PaginatedList<Voucher>> GetAllByAdminAsync(int pageNumber);
        Task<Voucher> GetByIdAsync(Guid customerId, Guid voucherId);

    }
}
