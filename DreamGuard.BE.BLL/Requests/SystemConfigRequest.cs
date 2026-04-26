using System;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.BLL.Requests
{
    public class SystemConfigCreateRequest
    {
        [Required]
        public string ConfigKey { get; set; } = string.Empty;
        [Required]
        public string ConfigValue { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<SystemConfigCreateRequest, SystemConfig>();
            }
        }
    }

    public class SystemConfigUpdateRequest
    {
        [Required]
        public string ConfigValue { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<SystemConfigUpdateRequest, SystemConfig>()
                    .ForMember(dest => dest.ConfigKey, opt => opt.Ignore());
            }
        }
    }
}
