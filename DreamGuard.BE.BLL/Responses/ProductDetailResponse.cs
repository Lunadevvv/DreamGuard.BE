using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;

namespace DreamGuard.BE.BLL.Responses
{
    public class ProductDetailResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public int? AgeGroup { get; set; }
        public int? WarrantyPolicyDay { get; set; }
        public int? ReturnPolicyDay { get; set; }
        public bool IsTradeInEligible { get; set; } = false;
        public decimal MinTradeInPrice { get; set; } = 0.0m;
        public decimal DepositAmount { get; set; } = 0.0m;
        public ProductStatus Status { get; set; }
        public FullyCustomizedProductType FullyCustomizedProductType { get; set; }
        public double AverageRating { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        public List<ProductVariantResponse> Variants { get; set; } = new List<ProductVariantResponse>();
    }
}