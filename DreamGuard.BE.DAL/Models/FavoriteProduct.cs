using System;
using System.Text.Json.Serialization;

namespace DreamGuard.BE.DAL.Models
{
    public class FavoriteProduct
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? ComboId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public Customer? Customer { get; set; }
        [JsonIgnore]
        public Product? Product { get; set; }
        [JsonIgnore]
        public Combo? Combo { get; set; }
    }
}
