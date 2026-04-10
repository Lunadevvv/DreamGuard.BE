using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class CalculateTradeInOrderPriceResponse
    {
        public decimal TradeInPrice { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal AmountToPay { get; set; }
    }
}
