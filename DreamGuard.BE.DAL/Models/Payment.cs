using System;
using System.Text.Json.Serialization;
using DreamGuard.BE.DAL.Constants;

namespace DreamGuard.BE.DAL.Models
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid? SoId { get; set; }
        public Guid? TradeInOrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public Guid? POrderId { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiredAt { get; set; }
        public PaymentType PaymentType { get; set; } = PaymentType.Purchase;
        public TradeInOrder? TradeInOrder { get; set; }
        public string? EvidenceUrl { get; set; }

        [JsonIgnore]
        public Order? POrder { get; set; }
        [JsonIgnore]
        public ServiceOrder? ServiceOrder { get; set; }
    }
}
