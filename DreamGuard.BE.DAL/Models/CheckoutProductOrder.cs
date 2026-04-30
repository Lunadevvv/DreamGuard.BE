using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using DreamGuard.BE.DAL.Constants;

namespace DreamGuard.BE.DAL.Models
{
    public class CheckoutProductOrder
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CheckoutOrderCode { get; set; } = string.Empty;
        public CheckoutOrderStatus Status { get; set; } = CheckoutOrderStatus.Pending;

        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAddonPrice { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal RefundingAmount { get; set; } = 0;
        public decimal RefundedAmount { get; set; } = 0;

        public Guid? UserVoucherId { get; set; }
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public Customer? Customer { get; set; }
        [JsonIgnore]
        public UserVoucher? UserVoucher { get; set; }
        [JsonIgnore]
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        [JsonIgnore]
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
