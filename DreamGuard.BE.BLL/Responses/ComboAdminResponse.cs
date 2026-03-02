using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Constants;

namespace DreamGuard.BE.BLL.Responses
{
    
    public class ComboAdminResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public ProductStatus Status { get; set; }
        public List<ComboChildAdminResponse>? ChildCombos { get; set; }
    }

    public class ComboChildAdminResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public ProductStatus Status { get; set; }
    }
}