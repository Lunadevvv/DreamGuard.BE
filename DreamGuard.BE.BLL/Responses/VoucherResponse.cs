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

        public decimal MinDiscountAmount { get; set; }

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
}
