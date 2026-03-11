using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class ForgotPasswordRequest
    {
        public required string PhoneNumber { get; set; }
    }
}