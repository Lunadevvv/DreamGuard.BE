using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface IStaffRepository : IGenericRepository<Staff>
    {
        Task<PaginatedList<Staff>> GetAllByAdminAsync(int pageNumber, int pageSize);
        Task<Staff?> GetByUserIdAsync(Guid userId);
    }
}
