using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Constants;
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
        public ProductStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid ProductId { get; set; }
    }

    public class ProductVariantGroupResponse
    {
        public string Color { get; set; } = string.Empty;
        public List<ProductVariantItemResponse> Variants { get; set; } = new();
    }

    public class ProductVariantItemResponse
    {
        public Guid Id { get; set; }
        public string Size { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public double SalePrice { get; set; }
        public double? BasePrice { get; set; }
        // public int StockQuantity { get; set; }
        // public string StockStatus { get; set; } = string.Empty; // "In Stock", "Low Stock", "Out of Stock"
    }

    public class ProductVariantAdminResponse
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalVariants { get; set; }
        public List<ProductVariantGroupResponse> ColorGroups { get; set; } = new();
    }
}