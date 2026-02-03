namespace DreamGuard.BE.API.Requests
{
    public class VerifyOtpRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
    }
}
