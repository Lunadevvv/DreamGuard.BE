using System.ComponentModel.DataAnnotations;
using DreamGuard.BE.DAL.Constants;

namespace DreamGuard.BE.BLL.Requests
{
    public class UpdateOrderStatusRequest
    {
        [Required(ErrorMessage = "Status is required.")]
        public OrderStatus Status { get; set; }
    }
}
