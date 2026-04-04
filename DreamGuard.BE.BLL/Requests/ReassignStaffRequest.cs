using System;
using System.ComponentModel.DataAnnotations;

namespace DreamGuard.BE.BLL.Requests
{
    public class ReassignStaffRequest
    {
        [Required]
        public Guid NewStaffId { get; set; }
    }
}
