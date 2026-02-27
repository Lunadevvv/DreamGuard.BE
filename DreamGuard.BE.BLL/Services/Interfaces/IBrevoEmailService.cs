using DreamGuard.BE.BLL.Common;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IBrevoEmailService
    {
        /// <summary>
        /// Send an email to a single recipient.
        /// </summary>
        Task<Result> SendEmailAsync(string to, string subject, string htmlContent);

        /// <summary>
        /// Send an email to multiple recipients.
        /// </summary>
        Task<Result> SendEmailAsync(List<string> to, string subject, string htmlContent);
        }
}
