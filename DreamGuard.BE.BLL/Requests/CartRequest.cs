using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DreamGuard.BE.DAL.ModelExtensions;

namespace DreamGuard.BE.BLL.Requests
{
    public class AddToCartRequest
    {
        public Guid? ProductVariantId { get; set; }
        public Guid? ComboId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        public List<ProductCustomizeDetailRequest> ProductCustomizeDetailRequest { get; set; } = new();
    }

    public class ProductCustomizeDetailRequest
    {
        [Required]
        public Guid ProductCustomizeTypeId { get; set; }

        [Required]
        public string CustomizeContent { get; set; } = string.Empty;
    }

    public class UpdateCartItemRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }

    public class SyncCartItemRequest
    {
        public Guid? ProductVariantId { get; set; }
        public Guid? ComboId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }

    public class SyncCartRequest
    {
        [Required]
        public List<SyncCartItemRequest> Items { get; set; } = new();
    }
}
