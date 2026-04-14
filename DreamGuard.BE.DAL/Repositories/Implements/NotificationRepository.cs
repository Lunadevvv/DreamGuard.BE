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
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(DreamGuardContext context) : base(context)
        {
        }

        public async Task<List<Notification>> GetByUserIdAsync(Guid userId)
        {
           return await _context.Notifications.Where(o => o.UserId == userId).ToListAsync();
        }

        public async Task<PaginatedList<Notification>> GetMyNotificationAsync(Guid userId, int pageNumber, int pageSize)
        {
            var query = _context.Notifications
                .Where(o => o.UserId == userId).OrderByDescending(o => o.CreatedAt);
            return await PaginatedList<Notification>.CreateAsync(query, pageNumber, pageSize);
        }
    }
}
