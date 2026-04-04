using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.BLL.Responses
{
    public class ProductVariantResponse
    {
        public Guid Id { get; set; }
        public string? Sku { get; set; }
        public decimal BasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public double? Weight { get; set; }
        public ProductAttribute? Attributes { get; set; }
        public string Size { get; set; } = string.Empty;
        public bool IsNew { get; set; }
        public bool IsCustomizable { get; set; }
        public ProductStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid ProductId { get; set; }
        public int StockQuantity { get; set; }
        public string StockStatus { get; set; } = string.Empty; // "In Stock", "Low Stock", "Out of Stock"
        public List<CustomizeCategoryGroupResponse> CustomizeOptionGroups { get; set; } = new();
    }
    public class ProductVariantSummaryResponse
    {
        public Guid Id { get; set; }
        public string? Sku { get; set; }
        public decimal BasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public double? Weight { get; set; }
        public ProductAttribute? Attributes { get; set; }
        public string Size { get; set; } = string.Empty;
        public bool IsNew { get; set; }
        public bool IsCustomizable { get; set; }
        public ProductStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid ProductId { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<ProductVariant, ProductVariantSummaryResponse>();
            }
        }
    }

    public class CustomizeCategoryGroupResponse
    {
        public CustomizeCategory Category { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<CustomizeOptionResponse> Options { get; set; } = new();
    }

    public class CustomizeOptionResponse
    {
        public Guid CustomizeTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public decimal DefaultPrice { get; set; }
        public decimal? OverridePrice { get; set; }
        public PriceCalculationMode CalculationMode { get; set; }
        public double? DefaultMultiplier { get; set; }
        public double? OverrideMultiplier { get; set; }
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
        public decimal SalePrice { get; set; }
        public decimal? BasePrice { get; set; }
        public int StockQuantity { get; set; }
        public string StockStatus { get; set; } = string.Empty; // "In Stock", "Low Stock", "Out of Stock"
        public string Status { get; set; } = string.Empty; // "Active", "Draft", "Hidden"
    }

    public class ProductVariantAdminResponse
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalVariants { get; set; }
        public List<ProductVariantGroupResponse> ColorGroups { get; set; } = new();
    }
}