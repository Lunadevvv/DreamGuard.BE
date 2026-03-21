using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class UserVoucher
    {
        public Guid UserVoucherId { get; set; } = Guid.NewGuid();
        public Guid CustomerId { get; set; }
        public Guid VoucherId { get; set; }
        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UsedAt { get; set; }
        public DateTime ExpiredAt { get; set; }

        public Customer Customer { get; set; }
        public Voucher Voucher { get; set; }
        public ServiceOrder? ServiceOrder { get; set; }
    }
}
