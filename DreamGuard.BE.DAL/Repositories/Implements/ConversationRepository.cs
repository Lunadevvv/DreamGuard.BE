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

        public async Task<PaginatedList<ChatMessage>> GetMessageHistoryAsync(Guid conversationId, Guid currentUserId, int pageNumber, int pageSize)
        {
            var query = _context.ChatMessages.Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.CreatedAt);
            var result = await PaginatedList<ChatMessage>.CreateAsync(query, pageNumber, pageSize);
            var unreadMessages = result.Items.Where(m => m.SenderId != currentUserId && !m.IsRead);
            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }
            await _context.SaveChangesAsync();
            return result;
        }
        public async Task<List<Guid>> GetAllUnreadConversationIds(List<Guid> conversationIds, Guid userId)
        {
            var unreadConversationIds = await _context.ChatMessages
                .Where(m => conversationIds.Contains(m.ConversationId)
                         && m.SenderId != userId
                         && !m.IsRead)
                .Select(m => m.ConversationId)
                .Distinct()
                .ToListAsync();
            return unreadConversationIds;
        }

        public async Task<PaginatedList<Conversation>> GetMyConversationAsync(Guid userId, int pageNumber, int pageSize)
        {
            var query = _context.Conversations.Include(c => c.TradeInOrder).Where(m => (m.StaffId == userId || m.CustomerId == userId) && m.TradeInOrder.Status == TradeInOrderStatus.NEGOTIATING);
            var result = await PaginatedList<Conversation>.CreateAsync(query, pageNumber, pageSize);
            return result;
        }

        public async Task MarkAsReadAsync(Guid conversationId, Guid currentUserId)
        {
            var result = await _context.ChatMessages.Where(m => m.ConversationId == conversationId && m.SenderId != currentUserId && !m.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));
        }

        public async Task<int> UpdateNegotiatingAsync(Guid tradeInOrderId, Guid staffId)
        {
            return await _context.TradeInOrders.Where(t => t.TradeInOrderId == tradeInOrderId && t.Status == TradeInOrderStatus.WAITING_FOR_STAFF)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.Status, TradeInOrderStatus.NEGOTIATING));
        }
    }
}
