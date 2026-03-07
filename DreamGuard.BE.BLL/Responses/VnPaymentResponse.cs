using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class VnPaymentResponse
    {
        public string PaymentId { get; set; } = "";
        public bool Success { get; set; } = false;
        public string Money { get; set; } = "";
        public string PaymentDescription { get; set; } = "";
        public string PaymentMethod { get; set; } = "";
        public string PaymentTime { get; set; } = "";
        public string VnpayTransactionId { get; set; } = "";
        public string VnPayResponseCode { get; set; } = "";
        public string VnPayTransactionStatus { get; set; } = "";

        public override string ToString()
        {
            return $"""
                PaymentId: {PaymentId}
                Success: {Success}
                Money: {Money}
                PaymentDescription: {PaymentDescription}
                PaymentMethod: {PaymentMethod}
                PaymentTime: {PaymentTime}
                VnpayTransactionId: {VnpayTransactionId}
                VnPayResponseCode: {VnPayResponseCode}
                VnPayTransactionStatus: {VnPayTransactionStatus}
            """;
        }
    }
}