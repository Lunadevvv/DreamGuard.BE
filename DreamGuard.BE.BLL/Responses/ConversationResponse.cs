using AutoMapper;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class ConversationResponse
    {
        public Guid ConversationId { get; set; } 
        public Guid TradeInOrderId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid StaffId { get; set; }
        public DateTime CreatedAt { get; set; }
        public TradeInOrderSummaryResponse TradeInOrder { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Conversation, ConversationResponse>()
                    .ForMember(dest => dest.TradeInOrder, opt => opt.MapFrom(src => src.TradeInOrder));
            }
        }
    }
}
