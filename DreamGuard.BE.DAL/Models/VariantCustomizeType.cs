using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class VariantCustomizeType
    {
        public Guid CusId { get; set; }
        public Guid ProductVariantId { get; set; }
        public decimal OverridePrice { get; set; }
        [JsonIgnore]
        public ProductVariant ProductVariant { get; set; }
        [JsonIgnore]
        public ProductCustomizeType ProductCustomizeType { get; set; }
    }
}