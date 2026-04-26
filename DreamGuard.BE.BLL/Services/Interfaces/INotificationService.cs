using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationToManagerAsync(Notification notification);
        Task SendNotificationAsync(Notification notification);
        Task<Result<PaginatedList<Notification>>> GetMyNotificationAsync(Guid userId, int pageNumber, int pageSize);
        Task<Result> MarkAsReadAsync(Guid userId);
    }
}
