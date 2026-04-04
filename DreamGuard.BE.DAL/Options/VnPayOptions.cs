using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Options
{
    public class VnPayOptions
    {
        public string Version { get; set; } = "2.1.0";
        public string Command { get; set; } = string.Empty;
        public string TmnCode { get; set; } = string.Empty;
        public string HashSecret { get; set; } = string.Empty;
        public string PaymentBackUrl { get; set; } = string.Empty;
        public string PaymentResultPage { get; set; } = string.Empty;
        public string Locale { get; set; } = "vn";
        public string CurrCode { get; set; } = "VND";
        public string BaseUrl { get; set; } = string.Empty;
        public string RefundUrl { get; set; } = "https://sandbox.vnpayment.vn/merchant_webapi/api/transaction";
        public int PaymentExpirationMinutes { get; set; } = 5;
    }
}