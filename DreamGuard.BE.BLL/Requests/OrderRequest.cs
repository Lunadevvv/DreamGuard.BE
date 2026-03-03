using System;
using System.ComponentModel.DataAnnotations;

namespace DreamGuard.BE.BLL.Requests
{
    public class CreateOrderRequest
    {
        [Required(ErrorMessage = "AddressId is required.")]
        public Guid AddressId { get; set; }

        public Guid? UserVoucherId { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
