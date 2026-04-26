using DreamGuard.BE.DAL.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class AuditLog
    {
        public Guid AuditLogId { get; set; } = Guid.NewGuid();  
        public Guid UserId { get; set; }
        public string ActionType { get; set; }
        public string Message { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string UserRole { get; set; } = Role.User;
        public User User { get; set; }
    }
}
