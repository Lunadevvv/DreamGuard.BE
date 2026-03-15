using AutoMapper;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class ServiceEvidenceResponse
    {
        public Guid SeId { get; set; }
        public Guid ServiceTaskId { get; set; }
        public string ImageUrl { get; set; }
        public string PublicId { get; set; }
        public string EvidenceType { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<ServiceEvidence, ServiceEvidenceResponse>();
            }
        }
    }
}
