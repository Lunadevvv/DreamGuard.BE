using DreamGuard.BE.DAL.Options;
using Microsoft.Extensions.Options;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;
using Task = System.Threading.Tasks.Task;

namespace DreamGuard.BE.BLL.Services
{
    public class BrevoEmailService : IBrevoEmailService
    {
        private readonly BrevoOptions _brevoOptions;
        public BrevoEmailService(IOptions<BrevoOptions> brevoOptions)
        {
            _brevoOptions = brevoOptions.Value;
            Configuration.Default.AddApiKey("api-key", _brevoOptions.ApiKey);
        }

        public async Task ActivateEmailAsync(string to, string otp)
        {
            var apiInstance = new TransactionalEmailsApi();
            // Template ID lấy trong Brevo dashboard
            long templateId = 3;
            var sendSmtpEmail = new SendSmtpEmail(
                to: new List<SendSmtpEmailTo> { new SendSmtpEmailTo(to) },
                templateId: templateId,
                _params: new Dictionary<string, object>
                {
                { "otp", otp },
                }
            );
            await apiInstance.SendTransacEmailAsync(sendSmtpEmail);
        }
    }
}
