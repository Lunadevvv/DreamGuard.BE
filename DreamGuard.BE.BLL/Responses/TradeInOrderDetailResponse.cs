using AutoMapper;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class TradeInOrderDetailResponse
    {
        public Guid TradeInOrderId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductVariantId { get; set; }
        public Guid POrderItemId { get; set; }
        public string OrderCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public TradeInOrderStatus Status { get; set; }
        public bool IsGood { get; set; }
        public string Description { get; set; }
        public string ReceiverName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public decimal TradeInPrice { get; set; }
        public decimal AmountToPay { get; set; }
        public decimal DepositAmount { get; set; }
        public List<PaymentSummaryResponse> Payments { get; set; } 
        public List<TradeInImage> TradeInImages { get; set; } = new List<TradeInImage>();
        public OrderItemResponse OrderItem { get; set; }
        public ProductVariantSummaryResponse ProductVariant { get; set; }
        public Conversation? Conversation { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<TradeInOrder, TradeInOrderDetailResponse>()
                    .ForMember(dest => dest.OrderItem, opt => opt.MapFrom(src => src.OrderItem))
                    .ForMember(dest => dest.ProductVariant, opt => opt.MapFrom(src => src.ProductVariant))
                    .ForMember(dest => dest.Payments, opt => opt.MapFrom(src => src.Payments))
                    .ForMember(dest => dest.Conversation, opt => opt.MapFrom(src => src.Conversation))
                    .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderItem != null ? src.OrderItem.OrderId : Guid.Empty));
            }
        }
    }
    public class TradeInOrderSummaryResponse
    {
        public Guid TradeInOrderId { get; set; }
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; }   
        public Guid CustomerId { get; set; }
        public Guid ProductVariantId { get; set; }
        public Guid POrderItemId { get; set; }
        public DateTime CreatedAt { get; set; }
        public TradeInOrderStatus Status { get; set; }
        public bool IsGood { get; set; }
        public string Description { get; set; }
        public string ReceiverName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public decimal TradeInPrice { get; set; }
        public decimal AmountToPay { get; set; }
        public decimal DepositAmount { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<TradeInOrder, TradeInOrderSummaryResponse>()
               .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderItem != null ? src.OrderItem.OrderId : Guid.Empty));
            }

        }
    }
}