using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql.Replication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class ChatMessageService : IChatMessageService
    {
        private readonly IChatMessageRepository _chatRepository;
        private readonly IConversationRepository _conversationRepository;
        private readonly UserManager<User> _usermanager;
        public ChatMessageService(IChatMessageRepository chatRepository, UserManager<User> userManager, IConversationRepository conversationRepository)
        {
            _conversationRepository = conversationRepository;
            _chatRepository = chatRepository;
            _usermanager = userManager;
        }
        public async Task<Result<ChatMessage>> SaveMessageAsync(Guid conversationId, Guid senderId, string messageText)
        {
            // Cần logic để biết sender này là Staff hay Customer (Dựa vào Role trong JWT JWT hoặc query DB)
            var user = await _usermanager.Users.FirstOrDefaultAsync(u => u.Id == senderId);
            if (user == null)
            {
                return Result<ChatMessage>.Failure("Sender not found.", 404);
            }
            var senderType = await _usermanager.GetRolesAsync(user);
            var role = senderType[0];

            var message = new ChatMessage
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Message = messageText,
                SenderType = role,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _chatRepository.CreateAsync(message);
            if(result == 0)
            {
                return Result<ChatMessage>.Failure("Failed to save message.", 500);
            }
            return Result<ChatMessage>.Success(message);
        }

        public async Task<Result> VerifyUserInConversationAsync(Guid conversationId, Guid userId)
        {
           var conversation = await _conversationRepository.GetByIdAsync(conversationId);
            if (conversation == null)
            {
                return Result.Failure("Conversation not found.", 404);
            }
            // Giả sử Conversation có 2 trường: CustomerId và StaffId
            if (conversation.CustomerId != userId && conversation.StaffId != userId)
            {
                return Result.Failure("User is not part of this conversation.", 400);
            }
            return Result.Success("Verify Successfully");
        }
    }
}
