using AutoMapper;
using DreamGuard.BE.DAL.Models;
using System.ComponentModel.DataAnnotations;

namespace DreamGuard.BE.BLL.Requests
{
    public class ProductTypeUpdateRequest
    {
        [Required]
        public string ProductTypeName { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a non-negative value.")]
        public decimal Price { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<ProductTypeUpdateRequest, ProductType>();
            }
        }
    }
}
