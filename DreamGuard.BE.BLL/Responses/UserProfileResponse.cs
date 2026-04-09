using AutoMapper;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.BLL.Responses
{
    public class UserProfileResponse
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public int MemberCoin {get; set;}

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Customer, UserProfileResponse>()
                    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                    .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber));
            }
        }
    }
}
