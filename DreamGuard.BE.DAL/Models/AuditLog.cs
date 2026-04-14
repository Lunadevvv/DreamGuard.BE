using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class AuditLog
    {
        public Guid AuditLogId { get; set; }
        public Guid UserId { get; set; }
        public string ActionType { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserRole { get; set; }
        public User User { get; set; }
    }
}
