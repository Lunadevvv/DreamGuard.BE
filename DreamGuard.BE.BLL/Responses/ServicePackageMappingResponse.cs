using AutoMapper;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class ServicePackageMappingResponse
    {
        public Guid ServicePackageMappingId { get; set; }
        public Guid ProductTypeId { get; set; }
        public Guid ServicePackageId { get; set; }
        public int Duration { get; set; }
        public decimal Price { get; set; }
        public string ProductTypeName { get; set; } 
        public string ServicePackageName { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<ServicePackageMapping, ServicePackageMappingResponse>()
                    .ForMember(dest => dest.ProductTypeName, opt => opt.MapFrom(src => src.ProductType.ProductTypeName))
                    .ForMember(dest => dest.ServicePackageName, opt => opt.MapFrom(src => src.ServicePackage.PackageName));
            }
        }
    }
}
