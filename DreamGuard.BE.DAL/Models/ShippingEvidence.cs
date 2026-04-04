using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class ShippingEvidence
    {
        public Guid EvidenceId { get; set; } = Guid.NewGuid();
        public Guid ShippingTaskId { get; set; }
        public string EvidenceUrl { get; set; } = string.Empty;
        public string EvidenceType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ShippingTask ShippingTask { get; set; } = null!;
    }
}
