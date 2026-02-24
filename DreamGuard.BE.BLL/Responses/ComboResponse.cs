using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class ComboResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int? AgeGroup { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public double BasePrice { get; set; }
        public double SalePrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public Guid? ComboParentId { get; set; }
    }
}
