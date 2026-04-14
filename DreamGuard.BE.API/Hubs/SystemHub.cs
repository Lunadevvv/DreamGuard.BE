using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace DreamGuard.BE.API.Hubs
{
    [Authorize]
    public class SystemHub : Hub
    {
        public SystemHub()
        {
        }

        public async Task JoinConversation(Guid conversationId)
        {
            if (!Guid.TryParse(Context.UserIdentifier, out Guid userId))
            {
                throw new HubException("Invalid User token or user not found");
            }
            if (Context.User.IsInRole($"{Role.Admin}"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }else throw new HubException("Only Admin can join this group");
        }

    }
}
