using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface IConversationRepository : IGenericRepository<Conversation>
    {
        Task MarkAsReadAsync(Guid conversationId, Guid currentUserId);
        Task<int> UpdateNegotiatingAsync(Guid tradeInOrderId, Guid staffId);
        Task<List<Guid>> GetAllUnreadConversationIds(List<Guid> conversationIds, Guid userId);
        Task<PaginatedList<ChatMessage>> GetMessageHistoryAsync(Guid conversationId, Guid currentUserId, int pageNumber, int pageSize);
        Task<PaginatedList<Conversation>> GetMyConversationAsync(Guid userId, int pageNumber, int pageSize);

    }
}
