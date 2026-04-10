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
        Task<int> UpdateNegotiatingAsync(Guid tradeInOrderId, Guid staffId);
        Task<PaginatedList<ChatMessage>> GetMessageHistoryAsync(Guid conversationId, int pageNumber, int pageSize);
        Task<PaginatedList<Conversation>> GetMyConversationAsync(Guid staffId, int pageNumber, int pageSize);

    }
}
