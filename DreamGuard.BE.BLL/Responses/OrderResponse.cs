using AutoMapper;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.BLL.Responses
{
    public class OrderItemResponse
    {
        public Guid Id { get; set; }
        public Guid? ProductVariantId { get; set; }
        public Guid? ComboId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Sku { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<OrderItem, OrderItemResponse>();
            }
        }
    }

    public class OrderResponse
    {
        public Guid Id { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string? Note { get; set; }

        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }

        public Guid? UserVoucherId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<OrderItemResponse> OrderItems { get; set; } = new();

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Order, OrderResponse>();
            }
        }
    }

    public class OrderListResponse
    {
        public Guid Id { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public int ItemCount { get; set; }
        public DateTime CreatedAt { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Order, OrderListResponse>()
                    .ForMember(dest => dest.ItemCount,
                        opt => opt.MapFrom(src => src.OrderItems.Count));
            }
        }
    }
}
