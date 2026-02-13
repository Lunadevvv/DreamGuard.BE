namespace DreamGuard.BE.BLL.Requests
{
    public class VerifyOtpRequest
    {
        public string phoneNumber { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
    }
}
