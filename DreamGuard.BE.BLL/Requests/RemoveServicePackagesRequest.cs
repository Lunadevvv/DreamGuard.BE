using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class RemoveServicePackagesRequest
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one service package ID must be provided.")]
        public List<Guid> ServicePackageIds { get; set; }
    }
}
