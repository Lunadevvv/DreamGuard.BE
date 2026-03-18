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
    public class BabyProfileRepository : GenericRepository<BabyProfile>, IBabyProfileRepository
    {
        public BabyProfileRepository(DreamGuardContext context) : base(context) { }

        public async Task<BabyProfile> GetByIdAsync(Guid customerId, Guid babyId)
        {
            try
            {
                return await _context.BabyProfiles.FirstOrDefaultAsync(bp => bp.CustomerId == customerId && bp.BabyId == babyId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving baby profiles for customer {customerId}: {ex.Message}");
            }
        }

        public async Task<PaginatedList<BabyProfile>> GetAllAsync(Guid customerId, int pageNumber)
        {
            try
            {
                var query = _context.BabyProfiles.Where(bp => bp.CustomerId == customerId);
                return await PaginatedList<BabyProfile>.CreateAsync(query, pageNumber, 4);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving baby profiles for customer {customerId}: {ex.Message}");
            }
        }
    }
}
