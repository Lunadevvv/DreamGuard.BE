using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IChatMessageService
    {
        Task<Result> VerifyUserInConversationAsync(Guid conversationId, Guid userId);
        Task<Result<ChatMessage>> SaveMessageAsync(Guid conversationId, Guid senderId, string messageText);
    }
}
