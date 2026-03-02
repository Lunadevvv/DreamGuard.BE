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
        public decimal TotalPrice { get; set; }
        public int? AvailableStock { get; set; }
    }

    public class CartResponse
    {
        public Guid Id { get; set; }
        public List<CartItemResponse> Items { get; set; } = new();
        public decimal CartTotal { get; set; }
        public int TotalItems { get; set; }
    }
}
