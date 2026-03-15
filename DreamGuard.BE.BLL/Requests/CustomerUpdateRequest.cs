using AutoMapper;
using DreamGuard.BE.DAL.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class CustomerUpdateRequest
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        public string Address { get; set; } 
        [Required]
        public string Gender { get; set; }
        [Required]
        public DateOnly? DateOfBirth { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<CustomerUpdateRequest, Customer>();
            }
        }
    }
}
