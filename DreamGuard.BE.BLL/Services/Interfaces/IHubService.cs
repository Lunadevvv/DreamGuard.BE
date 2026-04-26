using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public interface IHubService
    {
        Task SendAuditLogToAdmins(AuditLog audit);
        Task SendNotificationToStaff(Notification notification);
        Task SendNotificationToManager(Notification notification);
    }
}
