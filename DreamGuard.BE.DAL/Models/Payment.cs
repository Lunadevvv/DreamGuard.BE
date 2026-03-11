using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class Payment
    {
        public Guid PaymentId { get; set; } = Guid.NewGuid();

        public Guid SoId { get; set; }

        public string PaymentCode { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;

        public string PaymentStatus { get; set; } = string.Empty;

        public DateTime PaidAt { get; set; }

        public string Note { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = null;
        public ServiceOrder ServiceOrder { get; set; } = null!;
    }
}
