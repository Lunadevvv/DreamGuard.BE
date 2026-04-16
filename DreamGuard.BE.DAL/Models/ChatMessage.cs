using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class ChatMessage
    {
        public Guid ChatMessageId { get; set; } = Guid.NewGuid();
        public Guid ConversationId { get; set; }
        public Guid SenderId { get; set; }
        public string Message { get; set; }
        public string SenderType { get; set; } // "Customer" or "Staff"
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; } = false;
        [JsonIgnore]
        public Conversation Conversation { get; set; }
    }
}
