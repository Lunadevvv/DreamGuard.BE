using AutoMapper;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;

namespace DreamGuard.BE.BLL.Responses
{
    public class OrderItemResponse
    {
        public Guid Id { get; set; }
        public Guid? ProductVariantId { get; set; }
        public Guid? ComboId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsTradeInUsed { get; set; }
        public List<ProductCustomizeDetail> ProductCustomizeDetails { get; set; } = new();
        public string? CustomizeHash { get; set; }
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
        public string OrderCode { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAddonPrice { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? PaymentUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class OrderDetailResponse
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }

        public string ReceiverName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;

        public List<OrderItemResponse> Items { get; set; } = new();
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAddonPrice { get; set; }

        public string? VoucherCode { get; set; }
        public decimal? VoucherDiscountValue { get; set; }

        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class OrderSummaryResponse
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
