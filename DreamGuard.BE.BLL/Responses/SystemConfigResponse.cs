using System;
using AutoMapper;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.BLL.Responses
{
    public class SystemConfigResponse
    {
        public string ConfigKey { get; set; } = string.Empty;
        public string ConfigValue { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<SystemConfig, SystemConfigResponse>();
            }
        }
    }
}
