using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Constants;

namespace DreamGuard.BE.DAL.Models
{
    public class ProductCustomizeType
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public decimal DefaultPrice { get; set; }
        public CustomizeTypeStatus Status { get; set; }
        [JsonIgnore]
        public List<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
    }
}