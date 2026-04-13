using System;
using System.ComponentModel.DataAnnotations;

namespace DreamGuard.BE.BLL.Requests
{
    public class ShippingTaskCreateRequest
    {
        [Required]
        public Guid StaffId { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? TradeInOrderId { get; set; }
    }
}
