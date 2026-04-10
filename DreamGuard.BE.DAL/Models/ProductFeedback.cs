using System;
using System.Text.Json.Serialization;
using DreamGuard.BE.DAL.Constants;

namespace DreamGuard.BE.DAL.Models
{
    public class ProductFeedback
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProductId { get; set; }
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public int Score { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string Status { get; set; } = ProductFeedbackStatus.Visible;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [JsonIgnore]
        public Product Product { get; set; } = null!;
        [JsonIgnore]
        public Order Order { get; set; } = null!;
        [JsonIgnore]
        public Customer Customer { get; set; } = null!;
    }
}
