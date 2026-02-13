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
    public interface IBabyProfileRepository : IGenericRepository<BabyProfile>
    {
        Task<PaginatedList<BabyProfile>> GetAllAsync(string userId, int pageNumber);
        Task<BabyProfile> GetByIdAsync(string userId, string babyId);

    }
}
