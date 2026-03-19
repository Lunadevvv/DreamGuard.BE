using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class ServiceOrderDetailResponse
    {
        public Guid SoId { get; set; }
        public Guid? UserVoucherId { get; set; }
        public Guid CustomerId { get; set; }
        public string OrderCode { get; set; }
        public string CustomerNote { get; set; }
        public string ReceiverName { get; set; }

        public string Address { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime AppointmentDate { get; set; }

        public string Status { get; set; }

        public decimal TotalPrice { get; set; }
        public decimal SubTotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public List<ServiceOrderItemResponse> ServiceOrderItems { get; set; }
        public List<string> ImageUrl { get; set; }

    }
}
