using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class ServiceTaskCreateRequest
    {
        [Required]
        public Guid StaffId { get; set; }
        [Required]
        public Guid SoId { get; set; }
    }
}
