using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class CreateTradeInOrderResponse
    {
        public Guid TradeInOrderId { get; set; }
        public Guid PaymentId { get; set; }
        public string PaymentUrl { get; set; } = null!;
        public decimal TradeInPrice { get; set; }
        public decimal AmountToPay { get; set; }
        public decimal DepositAmount { get; set; }
        public DateTime ExpiredAt { get; set; }
    }
}
