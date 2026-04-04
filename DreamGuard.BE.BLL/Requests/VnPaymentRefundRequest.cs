using System;

namespace DreamGuard.BE.BLL.Requests
{
    public class VnPaymentRefundRequest
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string CreateBy { get; set; } = "System";
        public string IpAddress { get; set; } = "127.0.0.1";
        public string TransactionNo { get; set; } = "0"; // Mặc định truyền 0 nếu không lưu
    }
}
