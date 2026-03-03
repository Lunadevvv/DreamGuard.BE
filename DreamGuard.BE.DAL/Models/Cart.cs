using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DreamGuard.BE.DAL.Models
{
    public class Cart
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public User? User { get; set; }
        [JsonIgnore]
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
