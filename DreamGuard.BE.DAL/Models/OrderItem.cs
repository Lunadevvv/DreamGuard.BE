using System;
using System.Text.Json.Serialization;
using DreamGuard.BE.DAL.ModelExtensions;

namespace DreamGuard.BE.DAL.Models
{
    public class OrderItem
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid? ProductVariantId { get; set; }
        public Guid? ComboId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int TradeInUsedAmount { get; set; } = 0;
        public int ExchangeRequestedQuantity { get; set; } = 0;

        //Customize order details
        public List<ProductCustomizeDetail> ProductCustomizeDetails { get; set; } = new List<ProductCustomizeDetail>();
        public string? CustomizeHash { get; set; }
        
        [JsonIgnore]
        public Order? Order { get; set; }
        [JsonIgnore]
        public ProductVariant? ProductVariant { get; set; }
        [JsonIgnore]
        public Combo? Combo { get; set; }
        public ICollection<TradeInOrder> TradeInOrders { get; set; }
    }
}
