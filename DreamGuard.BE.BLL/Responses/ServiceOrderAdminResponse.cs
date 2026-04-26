using AutoMapper;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class ServiceOrderAdminResponse
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
        public StaffResponse? Staff { get; set; }
        public List<ServiceTaskResponse> ServiceTasks { get; set; }
        public RatingResponse? Rating { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<ServiceOrder, ServiceOrderAdminResponse>()
                     .ForMember(dest => dest.Staff, opt => opt.MapFrom(src => src.ServiceTasks == null ? null : src.ServiceTasks.OrderByDescending(st => st.CreatedAt).FirstOrDefault().Staff))
                     .ForMember(dest => dest.ServiceTasks, opt => opt.MapFrom(src => src.ServiceTasks))
                     .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating));
            }
        }
    }
}
