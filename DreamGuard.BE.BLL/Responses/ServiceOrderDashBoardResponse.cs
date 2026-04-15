using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class ServiceOrderDashBoardResponse
    {
        public int TotalServiceOrders { get; set; }
        public int TotalCancelledOrders { get; set; }
        public int TotalCompletedOrders { get; set; }
        public int TotalRejectedOrders { get; set; }
        public int TotalRefundOrders { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public decimal TotalVnPayAmount { get; set; }
        public decimal TotalCODAmount { get; set; }
    }
}
