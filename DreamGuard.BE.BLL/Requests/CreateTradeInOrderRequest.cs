using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class CreateTradeInOrderRequest
    {
        public Guid POrderItemId { get; set; }
        public Guid ProductVariantId { get; set; }
        public bool IsGood { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string ReceiverName { get; set; }
        [Required(ErrorMessage = "Phone number cannot be empty")]
        [RegularExpression(@"^(94|0)(3|5|7|8|9)\d{8}$", ErrorMessage = "Phone number doesn't have correct format")]
        public string PhoneNumber { get; set; }
        [Required]
        public string Address { get; set; }

    }
}
