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
        public Guid ServicePackageId { get; set; }
        public int Duration { get; set; } // Duration in minutes
        public decimal Price { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SuitableFor { get; set; } = string.Empty;
        public string ServiceContent { get; set; } = string.Empty;
        public string Benefits { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        private class Mapping : AutoMapper.Profile
        {
            public Mapping()
            {
                CreateMap<ServicePackageMapping, ServicePackageMappingResponse>()
                    .ForMember(dest => dest.PackageName, opt => opt.MapFrom(src => src.ServicePackage.PackageName))
                    .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.ServicePackage.Description))
                    .ForMember(dest => dest.SuitableFor, opt => opt.MapFrom(src => src.ServicePackage.SuitableFor))
                    .ForMember(dest => dest.ServiceContent, opt => opt.MapFrom(src => src.ServicePackage.ServiceContent))
                    .ForMember(dest => dest.Benefits, opt => opt.MapFrom(src => src.ServicePackage.Benefits))
                    .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ServicePackage.ImageUrl));
            }
        }
    }
}