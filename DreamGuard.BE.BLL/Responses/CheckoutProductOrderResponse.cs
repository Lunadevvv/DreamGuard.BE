using DreamGuard.BE.DAL.Constants;
using System;
using System.Collections.Generic;

namespace DreamGuard.BE.BLL.Responses
{
    public class CheckoutProductOrderResponse
    {
        public Guid Id { get; set; }
        public string CheckoutOrderCode { get; set; } = string.Empty;
        public CheckoutOrderStatus Status { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAddonPrice { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal RefundingAmount { get; set; }
        public decimal RefundedAmount { get; set; }

        public PaymentMethod PaymentMethod { get; set; }
        public string? PaymentUrl { get; set; }
        public Guid PaymentId { get; set; }
        public DateTime PaymentExpiredAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<OrderResponse> ChildOrders { get; set; } = new List<OrderResponse>();
    }
}
