using System;
using System.Text.Json.Serialization;
using DreamGuard.BE.DAL.ModelExtensions;

namespace DreamGuard.BE.DAL.Models
{
    public class CartItem
    {
        public Guid Id { get; set; }
        public Guid CartId { get; set; }
        public Guid? ProductVariantId { get; set; }
        public Guid? ComboId { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        //Product Customize Details
        public List<ProductCustomizeDetail> ProductCustomizeDetails { get; set; } = new List<ProductCustomizeDetail>();
        public string? CustomizeHash { get; set; }
        
        [JsonIgnore]
        public Cart? Cart { get; set; }
        [JsonIgnore]
        public ProductVariant? ProductVariant { get; set; }
        [JsonIgnore]
        public Combo? Combo { get; set; }
    }
}
