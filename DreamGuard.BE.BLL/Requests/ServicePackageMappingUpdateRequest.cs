using AutoMapper;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class ServicePackageMappingUpdateRequest
    {
        [Range(0, int.MaxValue, ErrorMessage = "Duration must be a non-negative integer.")]
        public int Duration { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a non-negative decimal.")]
        public decimal Price { get; set; }
        public ServicePackageUpdateRequest? ServicePackage { get; set; } 
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<ServicePackageMappingUpdateRequest, ServicePackageMapping>();
            }
        }
    }
}
