using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class CalculateTotalPriceRequest
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one product type must be selected")]
        public List<ProductTypeOrderRequest> ProductTypeOrderRequests { get; set; }
        public Guid ServicePackageId { get; set; }
        public Guid? UserVoucherId { get; set; }
    }
}