using AutoMapper;
using DreamGuard.BE.DAL.Models;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class ServiceOrderAdminResponse
    {
        public Guid SoId { get; set; }

        public Guid CustomerId { get; set; }

        public Guid ServicePackageMappingId { get; set; }

        public string OrderCode { get; set; }

        public string CustomNote { get; set; }
        public string ReceiverName { get; set; }

        public string Address { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime AppointmentDate { get; set; }

        public string Status { get; set; }

        public decimal TotalPrice { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<ServiceOrder, ServiceOrderAdminResponse>();
            }
        }
    }
}
