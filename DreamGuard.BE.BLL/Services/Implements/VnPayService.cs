using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.BLL.Utilities;
using DreamGuard.BE.DAL.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class VnPayService : IVnPayService
    {
        private readonly VnPayOptions _configuration;
        public VnPayService(IOptions<VnPayOptions> configuration)
        {
            _configuration = configuration.Value;
        }

        public string CreatePaymentUrl(VnPaymentRequest model)
        {
            var vnpay = new VnPayLibrary();
            var version = _configuration.Version!;
            var command = _configuration.Command!;
            var locale = _configuration.Locale!;
            var tmnCode = _configuration.TmnCode!;
            var currCode = _configuration.CurrCode!;
            var hashSecret = _configuration.HashSecret!;
            var baseUrl = _configuration.BaseUrl!;
            string returnUrl = $"{_configuration.PaymentBackUrl}";
            vnpay.AddRequestData("vnp_Version", version);
            vnpay.AddRequestData("vnp_Command", command);
            vnpay.AddRequestData("vnp_TmnCode", tmnCode);
            vnpay.AddRequestData("vnp_Amount", (model.Amount * 100).ToString()); 
            vnpay.AddRequestData("vnp_CreateDate", model.CreatedDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", currCode); 
            vnpay.AddRequestData("vnp_IpAddr", model.IpAddress);
            vnpay.AddRequestData("vnp_Locale", locale);
            vnpay.AddRequestData("vnp_OrderInfo", "Pay for the Order Code:" + model.OrderCode);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", model.PaymentId);
            var paymentUrl = vnpay.CreateRequestUrl(baseUrl, hashSecret);
            return paymentUrl;
        }

        public VnPaymentResponse GetPaymentResult(IQueryCollection collections)
        {
            var vnpay = new VnPayLibrary();

            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }
            var paymentId = vnpay.GetResponseData("vnp_TxnRef");
            var vnpayTranId = vnpay.GetResponseData("vnp_TransactionNo");
            var vnp_SecureHash = collections.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value;
            var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnp_CardType = vnpay.GetResponseData("vnp_CardType");
            var vnp_OrderInfo = vnpay.GetResponseData("vnp_OrderInfo");
            var vnp_Amount = vnpay.GetResponseData("vnp_Amount");
            var vnp_TransferTime = vnpay.GetResponseData("vnp_PayDate");
            var vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, _configuration.HashSecret!);
            if (!checkSignature || vnp_TransactionStatus != "00")
            {
                return new VnPaymentResponse
                {
                PaymentId = paymentId,
                Money = vnp_Amount,
                PaymentTime = vnp_TransferTime,
                Success = false,
                PaymentMethod = vnp_CardType,
                PaymentDescription = vnp_OrderInfo,
                VnpayTransactionId = vnpayTranId.ToString(),
                VnPayResponseCode = vnp_ResponseCode,
                VnPayTransactionStatus = vnp_TransactionStatus,
                };
            }
            // if (vnp_ResponseCode == "24")
            // {
            //     throw new CancelPaymentException();
            // }
            return new VnPaymentResponse
            {
                PaymentId = paymentId,
                Money = vnp_Amount,
                PaymentTime = vnp_TransferTime,
                Success = true,
                PaymentMethod = vnp_CardType,
                PaymentDescription = vnp_OrderInfo,
                VnpayTransactionId = vnpayTranId.ToString(),
                VnPayResponseCode = vnp_ResponseCode,
                VnPayTransactionStatus = vnp_TransactionStatus,
            };
        }
    }
}