using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class ComboProductVariant
    {
        public Guid Id { get; set; }
        public Guid ComboId { get; set; }
        public Guid ProductVariantId { get; set; }
        public int Quantity { get; set; }
        [JsonIgnore]
        public ProductVariant? ProductVariant { get; set; } 
        [JsonIgnore]
        public Combo? Combo { get; set; }
    }
}