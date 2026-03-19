using AutoMapper;
using DreamGuard.BE.DAL.Models;
using System.ComponentModel.DataAnnotations;

namespace DreamGuard.BE.BLL.Requests
{
    public class ProductTypeUpdateRequest
    {
        [Required]
        public string ProductTypeName { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<ProductTypeUpdateRequest, ProductType>();
            }
        }
    }
}
