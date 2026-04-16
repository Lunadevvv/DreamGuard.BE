using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class OrderDashBoardResponse
    {
        public int TotalOrders { get; set; }
        public int TotalCompletedOrders { get; set; }
        public int TotalCancelledOrders { get; set; }
        public int TotalRefundedOrders { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalCODAmount { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public decimal TotalVnPayAmount { get; set; }
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
    }
}
