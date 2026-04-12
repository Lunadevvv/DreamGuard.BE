using DreamGuard.BE.DAL.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class TradeInOrder
    {
        public Guid TradeInOrderId { get; set; } = Guid.NewGuid();
        public Guid CustomerId { get; set; }
        public Guid ProductVariantId { get; set; }
        public Guid POrderItemId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public TradeInOrderStatus Status { get; set; } = TradeInOrderStatus.Pending;
        public string OrderCode { get; set; } = string.Empty;
        public bool IsGood { get; set; }
        public string Description { get; set; }
        public string ReceiverName { get; set; }    
        public string PhoneNumber { get; set; } 
        public string Address { get; set; }
        public decimal TradeInPrice { get; set; }
        public decimal AmountToPay { get; set; }    
        public decimal DepositAmount { get; set; }  
        public List<TradeInImage> TradeInImages { get; set; } = new List<TradeInImage>();   
        public List<Payment> Payments { get; set; } = new List<Payment>();
        public OrderItem OrderItem { get; set; }
        public ProductVariant ProductVariant { get; set; }
        public Customer Customer { get; set; }
        public Conversation? Conversation { get; set; }
        public ICollection<ShippingTask> ShippingTasks { get; set; }
    }
}
