using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class BrevoEmailService : IBrevoEmailService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly BrevoOptions _brevoOptions;
        private readonly ILogger<BrevoEmailService> _logger;

        public BrevoEmailService(
            IHttpClientFactory httpClientFactory,
            IOptions<BrevoOptions> brevoOptions,
            ILogger<BrevoEmailService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _brevoOptions = brevoOptions.Value;
            _logger = logger;
        }

        public async Task<Result> SendEmailAsync(string to, string subject, string htmlContent)
        {
            return await SendEmailAsync(new List<string> { to }, subject, htmlContent);
        }

        public async Task<Result> SendEmailAsync(List<string> to, string subject, string htmlContent)
        {
            var emailContent = new
                {
                    sender = new { email = _brevoOptions.SenderEmail, name = _brevoOptions.SenderName },
                    to = to.Select(email => new { email = email }).ToList(),
                    subject = subject,
                    htmlContent = htmlContent
                };
            
            var json = JsonSerializer.Serialize(emailContent);

            return await SendRequestAsync(json);
        }

        private async Task<Result> SendRequestAsync(string payload)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("brevo");
                client.DefaultRequestHeaders.Add("api-key", _brevoOptions.ApiKey);

                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("smtp/email", content);

                if (response.IsSuccessStatusCode)
                {
                    return Result.Success("Email sent successfully");
                }

                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Brevo API error {StatusCode}: {Body}", (int)response.StatusCode, errorBody);
                return Result.Failure("Failed to send email", 500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email via Brevo");
                return Result.Failure("Failed to send email", 500);
            }
        }

    }
}
