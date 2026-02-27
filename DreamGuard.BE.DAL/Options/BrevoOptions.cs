namespace DreamGuard.BE.DAL.Options
{
    public class BrevoOptions
    {
        public const string BrevoOptionsKey = "BrevoOptions";
        public string BaseUrl { get; set; } = "https://api.brevo.com/v3/";
        public string ApiKey { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = "DreamGuard";
    }
}
