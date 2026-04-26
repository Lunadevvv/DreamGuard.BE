using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class VnPaymentRequest
    {
        public required string PaymentId { get; set; }
        public required string OrderCode { get; set; }
        public required string Description { get; set; }
        public required decimal Amount { get; set; }
        public required string IpAddress { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiredAt { get; set; } = DateTime.UtcNow.AddMinutes(5).AddHours(7);
    }
}