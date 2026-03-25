using System;
using System.Collections.Generic;
using DreamGuard.BE.DAL.ModelExtensions;

namespace DreamGuard.BE.BLL.Responses
{
    public class CartItemResponse
    {
        public Guid Id { get; set; }
        public Guid? ProductVariantId { get; set; }
        public Guid? ComboId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? Sku { get; set; }
        public string? ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }
        public int AvailableStock { get; set; }
        public bool IsAvailable { get; set; }
        public List<ProductCustomizeDetail> ProductCustomizeDetails { get; set; } = new();
        public decimal TotalAddOnPrice { get; set; }
    }

    public class CartResponse
    {
        public Guid CartId { get; set; }
        public List<CartItemResponse> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
    }
}
