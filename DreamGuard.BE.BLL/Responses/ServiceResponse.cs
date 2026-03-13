using AutoMapper;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class ServiceResponse
    {
        public Guid ServiceId { get; set; } 
        public string ServiceName { get; set; }
        public string Description { get; set; } 
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public int EstimatedDuration { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ICollection<ServiceAsset> ServiceAssets { get; set; } = null!;
        public ICollection<ServicePackage> ServicePackages { get; set; } = null!;
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Service, ServiceResponse>();
            }
        }
    }
}
