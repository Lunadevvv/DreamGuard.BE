using System;
using System.Collections.Generic;

namespace DreamGuard.BE.BLL.Responses
{
    public class FavoriteProductResponse
    {
        public Guid Id { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? ComboId { get; set; }
        public string ItemType { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public double AverageRating { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
    }
}
