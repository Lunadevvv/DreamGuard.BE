namespace DreamGuard.BE.BLL.Responses
{
    public class VnPaymentRefundResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ResponseCode { get; set; } = string.Empty;
        public string VnpayTransactionId { get; set; } = string.Empty;
    }
}
