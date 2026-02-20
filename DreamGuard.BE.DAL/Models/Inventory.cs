using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class Inventory
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int LowStockThreshold { get; set; } = 10;
        public Guid ProductVariantId { get; set; }
        [JsonIgnore]
        public ProductVariant? ProductVariant { get; set; }
    }
}