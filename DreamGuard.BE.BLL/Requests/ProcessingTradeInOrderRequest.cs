using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class ProcessingTradeInOrderRequest
    {
        public Guid TradeInOrderId { get; set; }
        public Guid StaffId { get; set; }
        public DateTime ShippingDate { get; set; }
    }
}
