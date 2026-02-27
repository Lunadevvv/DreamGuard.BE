using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class ComboProductItemResponse
    {
        public Guid ProductVariantId { get; set; }
        public string? Sku { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public int Quantity { get; set; }
    }

    public class ComboChildResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int? AgeGroup { get; set; }
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public double AverageRating { get; set; }
    }

    public class ComboDetailResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int? AgeGroup { get; set; }
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? ComboParentId { get; set; }

        // Populated only for child combos (ComboParentId != null)
        public List<ComboProductItemResponse>? ProductItems { get; set; }

        // Populated only for parent combos (ComboParentId == null)
        public List<ComboChildResponse>? ChildCombos { get; set; }
    }
}
