using AutoMapper;
using DreamGuard.BE.DAL.Models;
using System.ComponentModel.DataAnnotations;

namespace DreamGuard.BE.BLL.Requests
{
    public class ServiceUpdateRequest
    {
        [Required]
        public string ServiceName { get; set; }
        [Required]
        public string Description { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a non-negative value.")]
        public decimal Price { get; set; }
        [Required]
        public bool? IsActive { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "EstimatedDuration must be a non-negative integer.")]
        public int EstimatedDuration { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<ServiceUpdateRequest, Service>();
            }
        }
    }
}
