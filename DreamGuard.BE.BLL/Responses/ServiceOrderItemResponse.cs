using AutoMapper;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class ServiceOrderItemResponse
    {
        public Guid ServiceOrderItemId { get; set; }
        public Guid ServicePackageMappingId { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ServicePackageName { get; set; }
        public string ProductTypeName { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<ServiceOrderItem, ServiceOrderItemResponse>()
                    .ForMember(dest => dest.ServicePackageName, opt => opt.MapFrom(src => src.ServicePackageMapping.ServicePackage.PackageName))
                    .ForMember(dest => dest.ProductTypeName, opt => opt.MapFrom(src => src.ServicePackageMapping.ProductType.ProductTypeName));
            }
        }
    }
}
