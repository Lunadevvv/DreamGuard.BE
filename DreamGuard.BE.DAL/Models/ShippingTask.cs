using DreamGuard.BE.DAL.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class ShippingTask
    {
        public Guid ShippingTaskId { get; set; } = Guid.NewGuid();
        public Guid StaffId { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? TradeInOrderId { get; set; }
        public string Status { get; set; } = ShippingTaskStatus.Pending;
        public DateTime? ShippingDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string StaffNote { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<DamagedItem> DamagedItems { get; set; } = new List<DamagedItem>(); 
        public Staff Staff { get; set; } = null!;
        public Order? Order { get; set; }
        public TradeInOrder? TradeInOrder { get; set; }
        public ICollection<ShippingEvidence> ShippingEvidences { get; set; } = new List<ShippingEvidence>();
    }

    public class DamagedItem
    {
        public Guid OrderItemId { get; set; }
        public int DamagedQuantity { get; set; }
    }
}
