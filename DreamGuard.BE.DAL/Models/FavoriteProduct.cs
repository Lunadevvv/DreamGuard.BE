using System;
using System.Text.Json.Serialization;

namespace DreamGuard.BE.DAL.Models
{
    public class FavoriteProduct
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public User? User { get; set; }
        [JsonIgnore]
        public Product? Product { get; set; }
    }
}
