using AutoMapper;
using DreamGuard.BE.DAL.ModelExtensions;
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
        public string FullName { get; set; }  = null!;
        public string AvatarUrl { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public DateOnly DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }= null!;
        public string Email { get; set; }= null!;
        public int MemberCoin { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Customer, CustomerResponse>()
                    .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => $"{src.User.PhoneNumber}"))
                    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => $"{src.User.Email}"));
                
                CreateMap<PaginatedList<Customer>, PaginatedList<CustomerResponse>>()
                    .ConvertUsing((src, dest, context) =>
                    {
                        var customerResponses = src.Items.Select(c => context.Mapper.Map<CustomerResponse>(c)).ToList();
                        return new PaginatedList<CustomerResponse>(customerResponses, src.TotalCount, src.PageNumber, src.PageSize);
                    });
            }
        }
    }
}
