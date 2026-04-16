using CloudinaryDotNet.Actions;
using DreamGuard.BE.API.Hubs;
using DreamGuard.BE.BLL.Services.Implements;
using DreamGuard.BE.DAL.Models;
using Microsoft.AspNetCore.SignalR;

namespace DreamGuard.BE.API.Implements
{
    public class HubService : IHubService
    {
        private readonly IHubContext<NotiAndLogHub> _hubContext;
        public HubService(IHubContext<NotiAndLogHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public async Task SendAuditLogToAdmins(AuditLog audit)
        {
            await _hubContext.Clients.Group("Admins").SendAsync("ReceiveAuditLog", audit);
        }

        public async Task SendNotificationToStaff(Notification notification)
        {
            await _hubContext.Clients.User(notification.UserId.ToString()).SendAsync("ReceiveNotification", notification);
        }
        public async Task SendNotificationToManager(Notification notification)
        {
            await _hubContext.Clients.Group("Managers").SendAsync("ReceiveNotification", notification);
        }
    }
}
