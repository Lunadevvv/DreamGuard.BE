using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Constants;

namespace DreamGuard.BE.DAL.Models
{
    public class Combo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int? AgeGroup { get; set; }
        public double BasePrice { get; set; }
        public double SalePrice { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string ImagePublicId { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ProductStatus Status { get; set; } = ProductStatus.Draft;
        public Guid? ComboParentId { get; set; }
        [JsonIgnore]
        public Combo? ComboParent { get; set; }
        [JsonIgnore]
        public List<Combo> ComboChildrens { get; set; } = new List<Combo>();
        [JsonIgnore]
        public List<ComboProductVariant> ComboProductVariants { get; set; } = new List<ComboProductVariant>();
    }
}