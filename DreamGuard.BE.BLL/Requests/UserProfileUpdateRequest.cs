using AutoMapper;
using DreamGuard.BE.DAL.Models;
using System.ComponentModel.DataAnnotations;

namespace DreamGuard.BE.BLL.Requests
{
    public class UserProfileUpdateRequest
    {
        [Required(ErrorMessage = "Full name cannot be empty")]
        public string FullName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Email cannot be empty")]
        [EmailAddress(ErrorMessage = "Email doesn't have correct format")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Date of birth cannot be empty")]
        public DateOnly DateOfBirth { get; set; }
        [Required(ErrorMessage = "Gender cannot be empty")]
        public string Gender { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<UserProfileUpdateRequest, Customer>()
                    .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
                    .ForMember(dest => dest.User, opt => opt.Ignore())
                    .ForMember(dest => dest.ServiceOrders, opt => opt.Ignore())
                    .ForMember(dest => dest.BabyProfiles, opt => opt.Ignore())
                    .ForMember(dest => dest.Addresses, opt => opt.Ignore())
                    .ForMember(dest => dest.UserVouchers, opt => opt.Ignore())
                    .ForMember(dest => dest.Cart, opt => opt.Ignore())
                    .ForMember(dest => dest.Orders, opt => opt.Ignore())
                    .ForMember(dest => dest.FavoriteProducts, opt => opt.Ignore());
            }
        }
    }
}
