using AutoMapper;
using DreamGuard.BE.DAL.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DreamGuard.BE.BLL.Requests
{
    public class ServicePackageUpdateRequest
    {
        [Required]
        public string PackageName { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Duration must be a non-negative integer.")]
        public int Duration { get; set; }
        [Required]
        public string SuitableFor { get; set; }
        [Required]
        public string Benefits { get; set; }
        [Required]
        public string ServiceContent { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<ServicePackageUpdateRequest, ServicePackage>();
            }
        }
    }
}
