using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class RescheduleServiceOrderRequest
    {
        public Guid ServiceOrderId { get; set; }
        public Guid newStaffId { get; set; }
        public DateTime NewAppointmentDate { get; set; }
    }
}
