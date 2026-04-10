using AutoMapper;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class VoucherResponse
    {
        public string VoucherId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    
        public decimal DiscountValue { get; set; }

        public decimal MaxDiscountAmount { get; set; }
        public int RequiredCoin { get; set; }
        public string VoucherType { get; set; }

        public DateTime StartDate { get; set; }
     
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Voucher, VoucherResponse>();
            }
        }
    }

    public class UserVoucherResponse
    {
        public string UserVoucherId { get; set; }
        public string VoucherId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal MaxDiscountAmount { get; set; }
        public string VoucherType { get; set; }
        public DateTime ExpiredAt { get; set; }
        public bool IsUsed { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<UserVoucher, UserVoucherResponse>()
                    .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Voucher.Code))
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Voucher.Name))
                    .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Voucher.Description))
                    .ForMember(dest => dest.DiscountValue, opt => opt.MapFrom(src => src.Voucher.DiscountValue))
                    .ForMember(dest => dest.MaxDiscountAmount, opt => opt.MapFrom(src => src.Voucher.MaxDiscountAmount))
                    .ForMember(dest => dest.VoucherType, opt => opt.MapFrom(src => src.Voucher.VoucherType));
            }
        }
    }
}
