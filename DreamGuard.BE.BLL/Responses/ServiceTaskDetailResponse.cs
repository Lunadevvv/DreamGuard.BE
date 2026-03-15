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
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        // Additional fields from related entities
        // ServiceOrder fields
        public string ServiceOrderStatus { get; set; }
        public string CustomNote { get; set; }
        public string ReceiverName { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        public string Street { get; set; }
        public decimal TotalPrice { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime AppointmentDate { get; set; }
        // Package fields
        public string PackageName { get; set; }
        // Service fields
        public string ServiceName { get; set; }
        public string PaymentMethod { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<ServiceTask, ServiceTaskDetailResponse>()
                    .ForMember(dest => dest.ServiceOrderStatus, opt => opt.MapFrom(src => src.ServiceOrder.Status))
                    .ForMember(dest => dest.CustomNote, opt => opt.MapFrom(src => src.ServiceOrder.CustomNote))
                    .ForMember(dest => dest.ReceiverName, opt => opt.MapFrom(src => src.ServiceOrder.ReceiverName))
                    .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.ServiceOrder.Address))
                    .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.ServiceOrder.City))
                    .ForMember(dest => dest.District, opt => opt.MapFrom(src => src.ServiceOrder.District))
                    .ForMember(dest => dest.Ward, opt => opt.MapFrom(src => src.ServiceOrder.Ward))
                    .ForMember(dest => dest.Street, opt => opt.MapFrom(src => src.ServiceOrder.Street))
                    .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.ServiceOrder.TotalPrice))
                    .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.ServiceOrder.PhoneNumber))
                    .ForMember(dest => dest.AppointmentDate, opt => opt.MapFrom(src => src.ServiceOrder.AppointmentDate))
                    .ForMember(dest => dest.PackageName, opt => opt.MapFrom(src => src.ServiceOrder.ServicePackageMapping.ServicePackage.PackageName))
                    .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.ServiceOrder.ServicePackageMapping.Service.ServiceName))
                    .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.ServiceOrder.Payments.FirstOrDefault()!.PaymentMethod));
            }
        }
    }
}
