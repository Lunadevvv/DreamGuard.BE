using AutoMapper;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Implements;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class ConversationService : IConversationService
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IMapper _mapper;
        public ConversationService(IConversationRepository conversationRepository, IMapper mapper)
        {
            _conversationRepository = conversationRepository;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedList<ChatMessage>>> GetMessageHistoryAsync(Guid conversationId, int pageNumber, int pageSize)
        {
            var messages = await _conversationRepository.GetMessageHistoryAsync(conversationId, pageNumber, pageSize);
            messages.Items = messages.Items.Reverse().ToList();
            var paginatedResponse = new PaginatedList<ChatMessage>(messages.Items, messages.TotalCount, pageNumber, pageSize);
            return Result<PaginatedList<ChatMessage>>.Success(paginatedResponse);
        }
        public async Task<Result<PaginatedList<ConversationResponse>>> GetConversationAsync(Guid staffId, int pageNumber, int pageSize)
        {
            var conversations = await _conversationRepository.GetMyConversationAsync(staffId, pageNumber, pageSize);
            var conversationResponse = _mapper.Map<List<ConversationResponse>>(conversations.Items);
            var paginatedResponse = new PaginatedList<ConversationResponse>(conversationResponse, conversations.TotalCount, pageNumber, pageSize);
            return Result<PaginatedList<ConversationResponse>>.Success(paginatedResponse);
        }
    }
}