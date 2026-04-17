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
        private readonly IHubContext<ChatHub> _chatHubContext;
        public HubService(IHubContext<NotiAndLogHub> hubContext, IHubContext<ChatHub> chatHubContext)
        {
            _hubContext = hubContext;
            _chatHubContext = chatHubContext;
        }
        public async Task SendAuditLogToAdmins(AuditLog audit)
        {
            await _hubContext.Clients.Group("Admins").SendAsync("ReceiveAuditLog", audit);
            await _hubContext.Clients.Group("Managers").SendAsync("ReceiveAuditLog", audit);
        }

        public async Task SendNotificationToStaff(Notification notification)
        {
            await _hubContext.Clients.User(notification.UserId.ToString()).SendAsync("ReceiveNotification", notification);
        }
        public async Task SendNotificationToManager(Notification notification)
        {
            await _hubContext.Clients.Group("Managers").SendAsync("ReceiveNotification", notification);
            await _hubContext.Clients.Group("Admins").SendAsync("ReceiveNotification", notification);
        }

    }
}
    