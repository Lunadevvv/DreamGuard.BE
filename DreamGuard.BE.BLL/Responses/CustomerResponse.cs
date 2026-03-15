using AutoMapper;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class CustomerResponse
    {
        public Guid CustomerId { get; set; } 
        public string FullName { get; set; } 
        public string Address { get; set; } 
        public string AvatarUrl { get; set; } 
        public string Gender { get; set; } 
        public DateOnly DateOfBirth { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Customer, CustomerResponse>();
            }
        }
    }
}
