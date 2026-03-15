using AutoMapper;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class ServiceTaskResponse
    {
        public Guid ServiceTaskId { get; set; }
        public Guid StaffId { get; set; }
        public Guid SoId { get; set; }
        public string Status { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<ServiceTask, ServiceTaskResponse>();
            }
        }

    }
}
