using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services
{
    public interface IBrevoEmailService
    {
        Task ActivateEmailAsync(string to, string otp);
    }
}
