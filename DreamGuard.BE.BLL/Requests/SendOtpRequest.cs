namespace DreamGuard.BE.BLL.Requests
{
    public class SendOtpRequest
    {
        public string phone { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
    }
}
