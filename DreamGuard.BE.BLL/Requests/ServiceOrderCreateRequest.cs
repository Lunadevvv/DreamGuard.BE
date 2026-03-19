using DreamGuard.BE.DAL.Constants;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class ServiceOrderCreateRequest
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one product type must be selected")]
        public List<ProductTypeOrderRequest> ProductTypeOrderRequests { get; set; }
        public Guid ServicePackageId { get; set; }

        [Required(ErrorMessage = "Phone number cannot be empty")]
        [RegularExpression(@"^(94|0)(3|5|7|8|9)\d{8}$", ErrorMessage = "Phone number doesn't have correct format")]
        public string PhoneNumber { get; set; }
        [Required]
        public string ReceiverName { get; set; }

        [Required]
        public string Address { get; set; }
        [Required]
        public string CustomerNote { get; set; }
        [Required]
        public DateTime? AppointmentDate { get; set; }
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
        public Guid? UserVoucherId { get; set; }
    }
}
