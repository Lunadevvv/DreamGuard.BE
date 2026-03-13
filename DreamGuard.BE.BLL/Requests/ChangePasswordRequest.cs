using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class ChangePasswordRequest
    {
        public required string CurrentPassword { get; set; }
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).*$",ErrorMessage = "Password must contain uppercase letter, lowercase letter, digit, and special character")]
        public required string NewPassword { get; set; }
    }
}