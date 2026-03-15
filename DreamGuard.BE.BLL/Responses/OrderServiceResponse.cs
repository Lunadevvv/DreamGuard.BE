using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class OrderServiceResponse
    {
        public Guid ServiceOrderId { get; set; }
        public string PaymentUrl { get; set; } = null!;
        public decimal Price { get; set; }
        public DateTime ExpiredAt { get; set; }
    }
}
