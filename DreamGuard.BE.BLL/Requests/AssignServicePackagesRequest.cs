using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class AssignServicePackagesRequest
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one service package id and price is required.")]
        public List<ServicePackageMappingCreateRequest> Requests { get; set; } 
    }
}
