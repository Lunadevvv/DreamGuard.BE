using System.Text.Json.Serialization;

namespace DreamGuard.BE.DAL.Models
{
    public class CartItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CartId { get; set; }
        public Guid? ProductVariantId { get; set; }
        public Guid? ComboId { get; set; }
        public int Quantity { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public Cart? Cart { get; set; }
        [JsonIgnore]
        public ProductVariant? ProductVariant { get; set; }
        [JsonIgnore]
        public Combo? Combo { get; set; }
    }
}
