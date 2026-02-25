using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Constants;

namespace DreamGuard.BE.BLL.Responses
{
    public class ProductResponseForAdmin
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public int? AgeGroup { get; set; }
        public double AverageRating { get; set; }
        public double MaxPrice { get; set; } = 0.0;
        public double MinPrice { get; set; } = 0.0;
        public string CategoryName { get; set; } = string.Empty;
        public int VariantCount { get; set; }
        public ProductStatus Status { get; set; }
    }
}