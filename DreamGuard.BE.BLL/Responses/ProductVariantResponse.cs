using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.ModelExtensions;

namespace DreamGuard.BE.BLL.Responses
{
    public class ProductVariantResponse
    {
        public Guid Id { get; set; }
        public string? Sku { get; set; }
        public double BasePrice { get; set; }
        public double SalePrice { get; set; }
        public double? Weight { get; set; }
        public ProductAttribute? Attributes { get; set; }
        public string Size { get; set; } = string.Empty;
        public bool IsNew { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid ProductId { get; set; }
    }
}