using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IConversationService
    {
        Task<Result<PaginatedList<ChatMessage>>> GetMessageHistoryAsync(Guid conversationId, int pageNumber, int pageSize);
        Task<Result<PaginatedList<ConversationResponse>>> GetConversationAsync(Guid staffId, int pageNumber, int pageSize);
    }
}
