using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace DreamGuard.BE.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatMessageService _chatService;

        public ChatHub(IChatMessageService chatService)
        {
            _chatService = chatService;
        }

        // Client gọi hàm này khi mở màn hình chat
        public async Task JoinConversation(Guid conversationId)
        {
            if(!Guid.TryParse(Context.UserIdentifier, out Guid userId))
            {
                throw new HubException("Invalid User token or user not found");
            }
            // (Tùy chọn) Validate xem userId này có quyền tham gia conversation này không
            var result = await _chatService.VerifyUserInConversationAsync(conversationId, userId);
            if (!result.Succeeded)
            {
                throw new HubException(result.Error);
            }
            // Đưa connection này vào group có tên là ConversationId
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
        }

        // Client gọi hàm này để gửi tin nhắn
        public async Task SendMessage(Guid conversationId, string message)
        {
            if (!Guid.TryParse(Context.UserIdentifier, out Guid userId))
            {
                throw new HubException("Invalid User token or user not found");
            }

            // 1. Lưu tin nhắn vào Database (Bảng ChatMessage)
            var result = await _chatService.SaveMessageAsync(conversationId, userId, message);
            if (!result.Succeeded)
            {
                throw new HubException(result.Error);
            }
            // 2. Broadcast tin nhắn tới những ai đang trong Group này
            await Clients.Group(conversationId.ToString()).SendAsync("ReceiveMessage", result.Data);
        }

        public async override Task OnDisconnectedAsync(Exception? exception)
        {
            // Logic khi người dùng mất kết nối hoặc đóng trình duyệt
            var userId = Context.UserIdentifier;
            await Clients.Others.SendAsync("UserOffline", userId);

            await base.OnDisconnectedAsync(exception);
        }
        // Gửi tín hiệu đang gõ cho một người dùng cụ thể
        public async Task SendTypingSignal(string receiverId, bool isTyping)
        {
            string senderId = Context.ConnectionId; // Hoặc User Id của bạn
            await Clients.User(receiverId).SendAsync("ReceiveTypingStatus", senderId, isTyping);
        }
    }
}
