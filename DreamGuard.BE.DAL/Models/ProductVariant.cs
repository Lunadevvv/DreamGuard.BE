using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;

namespace DreamGuard.BE.DAL.Models
{
    public class ProductVariant
    {
        public Guid Id { get; set; }
        public string? Sku { get; set; }
        public required decimal BasePrice { get; set; }
        public required decimal SalePrice { get; set; }
        public double? Weight { get; set; }
        public ProductAttribute? Attributes { get; set; }
        public string Size { get; set; } = string.Empty;
        public ProductStatus Status { get; set; } = ProductStatus.Draft;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsNew { get; set; } = true;
        public bool IsCustomizable { get; set; } = false;
        public Guid ProductId { get; set; }
        [JsonIgnore]
        public Product? Product { get; set; }
        [JsonIgnore]
        public Inventory? Inventory { get; set; }
        [JsonIgnore]
        public List<ComboProductVariant> ComboProductVariants { get; set; } = new List<ComboProductVariant>();
        [JsonIgnore]
        public List<VariantCustomizeType> VariantCustomizeTypes { get; set; } = new List<VariantCustomizeType>();
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public List<TradeInOrder> TradeInOrders { get; set; } = new List<TradeInOrder>();
    }
}