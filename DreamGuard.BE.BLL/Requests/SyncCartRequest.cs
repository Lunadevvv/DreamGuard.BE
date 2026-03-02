using System.ComponentModel.DataAnnotations;

namespace DreamGuard.BE.BLL.Requests
{
    public class SyncCartItemRequest
    {
        public Guid? ProductVariantId { get; set; }
        public Guid? ComboId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }

    public class SyncCartRequest
    {
        [Required(ErrorMessage = "Items list is required.")]
        public List<SyncCartItemRequest> Items { get; set; } = new();
    }
}
