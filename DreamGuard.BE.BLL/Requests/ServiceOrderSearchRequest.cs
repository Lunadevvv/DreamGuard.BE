using DreamGuard.BE.DAL.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class ServiceOrderSearchRequest
    {
        public string? OrderCode { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
    }
}
