using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface IStaffRepository : IGenericRepository<Staff>
    {
        Task<Staff?> GetByUserIdAsync(Guid userId);
    }
}
