using System;
using System.ComponentModel.DataAnnotations;
using DreamGuard.BE.DAL.Constants;

namespace DreamGuard.BE.BLL.Requests
{
    public class CreateOrderRequest
    {
        [Required(ErrorMessage = "AddressId is required.")]
        public Guid AddressId { get; set; }

        public Guid? UserVoucherId { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "ShippingFee must be non-negative.")]
        public decimal ShippingFee { get; set; }

        [Required(ErrorMessage = "PaymentMethod is required.")]
        public PaymentMethod PaymentMethod { get; set; }
    }

    public class OrderLineItemRequest
    {
        public Guid? ProductVariantId { get; set; }
        public Guid? ComboId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        public List<ProductCustomizeDetailRequest> ProductCustomizeDetailRequest { get; set; } = new();
    }

    public class CreateOrderByAdminRequest
    {
        [Required(ErrorMessage = "CustomerId is required.")]
        public Guid CustomerId { get; set; }

        [Required(ErrorMessage = "AddressId is required.")]
        public Guid AddressId { get; set; }

        public Guid? UserVoucherId { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;

        [Required(ErrorMessage = "Items are required.")]
        [MinLength(1, ErrorMessage = "At least one item is required.")]
        public List<OrderLineItemRequest> Items { get; set; } = new();
    }
}
