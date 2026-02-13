using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class RegisterResponse
    {
        public Guid UserId { get; set; } = Guid.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
