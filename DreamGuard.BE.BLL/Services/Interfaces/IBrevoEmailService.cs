using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IBrevoEmailService
    {
        Task<Result> ActivateEmailAsync(string to, string otp);
        Task<Result> SendCustomEmailAsync(string to, string subject, string content);
    }
}
