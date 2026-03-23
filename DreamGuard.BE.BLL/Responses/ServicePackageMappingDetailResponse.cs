using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class ServicePackageMappingDetailResponse
    {
        public Guid ServicePackageMappingId { get; set; }
        public Guid ProductTypeId { get; set; }
        public Guid ServicePackageId { get; set; }
        public int Duration { get; set; } // Duration in minutes
        public decimal Price { get; set; }
        public string ProductTypeName { get; set; }
        public string PackageName { get; set; }
        private class Mapping : AutoMapper.Profile
        {
            public Mapping()
            {
                CreateMap<ServicePackageMapping, ServicePackageMappingDetailResponse>()
                    .ForMember(dest => dest.ProductTypeName, opt => opt.MapFrom(src => src.ProductType.ProductTypeName))
                    .ForMember(dest => dest.PackageName, opt => opt.MapFrom(src => src.ServicePackage.PackageName));
            }
        }
    }
}