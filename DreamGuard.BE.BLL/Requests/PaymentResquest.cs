using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class RefundPaymentRequest
    {
        public Guid? OrderId { get; set; }
        public Guid? TradeInOrderId { get; set; }
        [Required(ErrorMessage = "Reason is required.")]
        public string Reason { get; set; }
        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }
    }
}