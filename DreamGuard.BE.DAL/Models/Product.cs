using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;

namespace DreamGuard.BE.DAL.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        [Range(0, int.MaxValue, ErrorMessage = "AgeGroup must be a non-negative value.")]
        public int? AgeGroup { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Warranty must be a non-negative value.")]
        public int? WarrantyPolicyDay { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Return Policy must be a non-negative value.")]
        public int? ReturnPolicyDay { get; set; }
        public ProductStatus Status { get; set; } = ProductStatus.Draft;
        public FullyCustomizedProductType FullyCustomizedProductType { get; set; } = FullyCustomizedProductType.None;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public double AverageRating { get; set; } = 0.0;
        public bool IsTradeInEligible { get; set; } = false;
        public decimal MinTradeInPrice { get; set; } = 0.0m;
        public decimal DepositAmount { get; set; } = 0.0m;
        public int? CateId { get; set; }
        [JsonIgnore]
        public Category? Category { get; set; }
        [JsonIgnore]
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public ICollection<ProductAsset> Assets { get; set; } = new List<ProductAsset>();
        public ICollection<ProductCertificate> Certificates { get; set; } = new List<ProductCertificate>();
    }
}