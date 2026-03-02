using System.Text.Json.Serialization;

namespace DreamGuard.BE.DAL.Models
{
    public class OrderItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }
        public Guid? ProductVariantId { get; set; }
        public Guid? ComboId { get; set; }

        // Snapshotted product info
        public string ProductName { get; set; } = string.Empty;
        public string? Sku { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }

        [JsonIgnore]
        public Order? Order { get; set; }
        [JsonIgnore]
        public ProductVariant? ProductVariant { get; set; }
        [JsonIgnore]
        public Combo? Combo { get; set; }
    }
}
