using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.ModelExtensions;

namespace DreamGuard.BE.BLL.Requests
{
    public class ProductVariantRequest
    {
        public string? Sku { get; set; }
        public required double BasePrice { get; set; }
        public required double SalePrice { get; set; }
        public double? Weight { get; set; }
        public ProductAttribute? Attributes { get; set; }
        public bool IsNew { get; set; } = true;
        public Guid ProductId { get; set; }
    }
}