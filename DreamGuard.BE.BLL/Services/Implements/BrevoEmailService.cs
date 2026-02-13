using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Options;
using Microsoft.Extensions.Options;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;
using Task = System.Threading.Tasks.Task;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class BrevoEmailService : IBrevoEmailService
    {
        private readonly BrevoOptions _brevoOptions;
        public BrevoEmailService(IOptions<BrevoOptions> brevoOptions)
        {
            _brevoOptions = brevoOptions.Value;
            Configuration.Default.AddApiKey("api-key", _brevoOptions.ApiKey);
        }

        public async Task<Result> ActivateEmailAsync(string to, string otp)
        {
            try
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
                return Result.Success("Send mail successfully");
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to send activation email: {ex.Message}", 500);
            }
        }

        public async Task<Result> SendCustomEmailAsync(string to, string subject, string content)
        {
            try
            {
                var apiInstance = new TransactionalEmailsApi();
                var sendSmtpEmail = new SendSmtpEmail(
                    to: new List<SendSmtpEmailTo> { new SendSmtpEmailTo(to) },
                    subject: subject,
                    htmlContent: content
                );
                await apiInstance.SendTransacEmailAsync(sendSmtpEmail);
                return Result.Success("Send mail successfully");
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to send activation email: {ex.Message}", 500);
            }
        }
    }
}
