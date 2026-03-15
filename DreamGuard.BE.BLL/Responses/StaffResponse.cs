using AutoMapper;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class StaffResponse
    {
        public Guid StaffId { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string AvatarUrl { get; set; }
        public string Position { get; set; }
        public string Gender { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Status { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Staff, StaffResponse>();
            }
        }
    }
}
