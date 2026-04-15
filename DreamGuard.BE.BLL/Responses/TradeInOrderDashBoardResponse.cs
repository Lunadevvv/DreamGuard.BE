using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class TradeInOrderDashBoardResponse
    {
        public int TotalTradeInOrders { get; set; }
        public int TotalCompletedTradeInOrders { get; set; }
        public int TotalCancelledTradeInOrders { get; set; }
        public int TotalRefundedTradeInOrders { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPurchaseAmount { get; set; }
        public decimal TotalDepositAmount { get; set; }
        public decimal TotalCODAmount { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public decimal TotalVnPayAmount { get; set; }
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

    }
}
