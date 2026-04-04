using DreamGuard.BE.DAL.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class AdminSearchTradeInOrderRequest
    {
        public Guid? CustomerId { get; set; }

        public Guid? ProductVariantId { get; set; }

        public TradeInOrderStatus? Status { get; set; }

        public bool? IsGood { get; set; }

        public decimal? TradeInPrice { get; set; }

        public decimal? AmountToPay { get; set; }

        public decimal? DepositAmount { get; set; }

        public string? PhoneNumber { get; set; }
    }
}
