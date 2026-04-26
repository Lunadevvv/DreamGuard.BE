using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class Notification
    {
        public Guid NotificationId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string ActionType { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public User User { get; set; }
    }
}
