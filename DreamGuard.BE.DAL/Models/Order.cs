using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;

namespace DreamGuard.BE.DAL.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Address snapshot
        public string ReceiverName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;

        // Price summary
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAddonPrice { get; set; }
        // Voucher
        public Guid? UserVoucherId { get; set; }

        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        [JsonIgnore]
        public Customer? Customer { get; set; }
        [JsonIgnore]
        public UserVoucher? UserVoucher { get; set; }
        public Guid? CheckoutProductOrderId { get; set; }
        public decimal ShippingFee { get; set; }
        
        [JsonIgnore]
        public CheckoutProductOrder? CheckoutProductOrder { get; set; }
        [JsonIgnore]
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        [JsonIgnore]
        public List<Payment> Payments { get; set; } = new List<Payment>();
        [JsonIgnore]
        public ICollection<ShippingTask> ShippingTasks { get; set; } = new List<ShippingTask>();
        [JsonIgnore]
        public ICollection<ProductFeedback> ProductFeedbacks { get; set; } = new List<ProductFeedback>();
    }
}
