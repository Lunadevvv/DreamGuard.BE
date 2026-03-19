using AutoMapper;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class ServiceTaskDetailResponse
    {
        public Guid ServiceTaskId { get; set; }
        public Guid StaffId { get; set; }
        public Guid SoId { get; set; }
        public string Status { get; set; }
        public string StaffNote { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }

        // Additional fields from related entities
        // ServiceOrder fields
        public string ServiceOrderStatus { get; set; }
        public string CustomerNote { get; set; }
        public string ReceiverName { get; set; }

        public string Address { get; set; }
        public decimal TotalPrice { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public List<ServiceOrderItemResponse> ServiceOrderItems { get; set; }
        public List<string> ServiceOrderImageUrl { get; set; }
    }
}
