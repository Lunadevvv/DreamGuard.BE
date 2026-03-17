using AutoMapper;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class ProductTypeResponse
    {
        public Guid ProductTypeId { get; set; } 
        public string ProductTypeName { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<ProductType, ProductTypeResponse>();
            }
        }
    }
}
