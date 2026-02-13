using AutoMapper;
using DreamGuard.BE.DAL.Models;
using System.ComponentModel.DataAnnotations;

namespace DreamGuard.BE.BLL.Requests
{
    public class AddressUpdateRequest
    {
        [Required]
        public string ReceiverName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Phone number cannot be empty")]
        [RegularExpression(@"^(94|0)(3|5|7|8|9)\d{8}$", ErrorMessage = "Phone number doesn't have correct format")]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required]
        public string Street { get; set; } = string.Empty;
        [Required]
        public string City { get; set; } = string.Empty;
        [Required]
        public string District { get; set; } = string.Empty;
        [Required]
        public string Ward { get; set; } = string.Empty;
        [Required]
        public string Province { get; set; } = string.Empty;

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<AddressUpdateRequest, Address>();
            }
        }
    }
}
