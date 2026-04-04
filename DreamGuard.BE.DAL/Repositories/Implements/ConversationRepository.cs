using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.DbContext;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Repositories.Implements
{
    public class ConversationRepository : GenericRepository<Conversation>, IConversationRepository
    {
        public ConversationRepository(DreamGuardContext context) : base(context)
        {
        }

        public async Task<PaginatedList<ChatMessage>> GetMessageHistoryAsync(Guid conversationId, int pageNumber, int pageSize)
        {
            var query = _context.ChatMessages.Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.CreatedAt);
            return await PaginatedList<ChatMessage>.CreateAsync(query, pageNumber, pageSize);
        }

        public async Task<PaginatedList<Conversation>> GetMyConversationAsync(Guid staffId, int pageNumber, int pageSize)
        {
            var query = _context.Conversations.Include(c => c.TradeInOrder).Where(m => m.StaffId == staffId && m.TradeInOrder.Status == TradeInOrderStatus.WAITING_FOR_STAFF);
            return await PaginatedList<Conversation>.CreateAsync(query, pageNumber, pageSize);
        }

        public async Task<int> UpdateNegotiatingAsync(Guid tradeInOrderId, Guid staffId)
        {
            return await _context.TradeInOrders.Where(t => t.TradeInOrderId == tradeInOrderId && t.Status == TradeInOrderStatus.WAITING_FOR_STAFF)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.Status, TradeInOrderStatus.NEGOTIATING));
        }
    }
}
