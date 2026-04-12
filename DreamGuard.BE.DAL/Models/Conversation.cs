using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class Conversation
    {
        public Guid ConversationId { get; set; } = Guid.NewGuid();
        public Guid TradeInOrderId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid StaffId { get; set; }
        public DateTime CreatedAt { get; set; }
        [JsonIgnore]
        public TradeInOrder TradeInOrder { get; set; }
        [JsonIgnore]
        public Customer Customer { get; set; }
        [JsonIgnore]
        public Staff Staff { get; set; }
        [JsonIgnore]
        public ICollection<ChatMessage> ChatMessages { get; set; }
    }
}
