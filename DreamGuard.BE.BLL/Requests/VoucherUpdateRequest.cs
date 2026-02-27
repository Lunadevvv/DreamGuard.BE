using AutoMapper;
using DreamGuard.BE.DAL.Models;
using System.ComponentModel.DataAnnotations;

namespace DreamGuard.BE.BLL.Requests
{
    public class VoucherUpdateRequest
    {
        [Required]
        public string Code { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Range(0.01, double.MaxValue)]
        public decimal DiscountValue { get; set; }
        [Range(0.01, double.MaxValue)]
        public decimal MaxDiscountAmount { get; set; }
        [Range(0.01, double.MaxValue)]
        public decimal MinDiscountAmount { get; set; }
        [Required]
        public DateTime? StartDate { get; set; }
        [Required]
        public DateTime? EndDate { get; set; }
        [Required]
        public bool? IsActive { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<VoucherUpdateRequest, Voucher>();
            }
        }
    }
}
