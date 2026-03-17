using Microsoft.AspNetCore.Antiforgery;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class ForcedCancelledRequest
    {
        [Required]
        public string StaffNote { get; set; }
    }
}
