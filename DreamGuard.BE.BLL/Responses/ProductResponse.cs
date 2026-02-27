using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class ProductResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public int? AgeGroup { get; set; }
        public double AverageRating { get; set; }
        public decimal BasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}