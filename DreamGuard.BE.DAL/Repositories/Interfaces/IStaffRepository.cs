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
    public interface IStaffRepository : IGenericRepository<Staff>
    {
        Task<PaginatedList<Staff>> GetAllByAdminAsync(int pageNumber, int pageSize);
    }
}
