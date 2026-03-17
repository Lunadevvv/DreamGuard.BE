using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.DbContext;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DreamGuard.BE.DAL.Repositories.Implements
{
    public class StaffRepository : GenericRepository<Staff>, IStaffRepository
    {
        public StaffRepository(DreamGuardContext context) : base(context) { }

        public async Task<PaginatedList<Staff>> GetAllByAdminAsync(int pageNumber, int pageSize)
        {
            try
            {
                var query = _context.Staffs;
                return await PaginatedList<Staff>.CreateAsync(query, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving Staff for admin");
            }
        }

        public async Task<Staff?> GetByUserIdAsync(Guid userId)
        {
            return await _context.Staffs.FirstOrDefaultAsync(s => s.StaffId == userId);
        }
    }
}
