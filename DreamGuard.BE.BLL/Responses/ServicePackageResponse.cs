using AutoMapper;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class ServicePackageResponse
    {
        public Guid ServicePackageId { get; set; }
        public string PackageName { get; set; }
        public decimal Price { get; set; }
        public ServicePackageStatus Status { get; set; }
        public int Duration { get; set; }
        public string SuitableFor { get; set; }
        public string Benefits { get; set; }
        public string ServiceContent { get; set; }
        public string ImageUrl { get; set; }
        public string PublicId { get; set; }
        public ProductType? Service { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<ServicePackage, ServicePackageResponse>();
            }
        }
    }
}
