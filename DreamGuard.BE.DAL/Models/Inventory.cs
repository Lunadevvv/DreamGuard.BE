using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class Inventory
    {
        public Guid Id { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative integer.")]
        public int Quantity { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Defect quantity must be a non-negative integer.")]
        public int DefectQuantity { get; set; } = 0;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        [Range(0, int.MaxValue, ErrorMessage = "Low stock threshold must be a non-negative integer.")]
        public int LowStockThreshold { get; set; } = 10;
        public Guid ProductVariantId { get; set; }
        [JsonIgnore]
        public ProductVariant? ProductVariant { get; set; }
    }
}