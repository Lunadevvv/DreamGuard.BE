using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.BLL.Requests
{
    public class CreateProductRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "Slug is required.")]
        public string Slug { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        [Range(0, int.MaxValue, ErrorMessage = "AgeGroup must be a non-negative value.")]
        public int? AgeGroup { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Warranty must be a non-negative value.")]
        public int? WarrantyPolicyDay { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Return Policy must be a non-negative value.")]
        public int? ReturnPolicyDay { get; set; }
        public FullyCustomizedProductType FullyCustomizedProductType { get; set; } = FullyCustomizedProductType.None;
        public int? CateId { get; set; }
        public List<Guid> CertificateIds { get; set; } = new List<Guid>();
        public bool IsTradeInEligible { get; set; } = false;
        [Range(0, double.MaxValue, ErrorMessage = "MinTradeInPrice must be a non-negative value.")]
        public decimal MinTradeInPrice { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "DepositAmount must be a non-negative value.")]
        public decimal DepositAmount { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<CreateProductRequest, Product>()
                    .ForMember(dest => dest.Certificates, opt => opt.Ignore());
            }
        }
    }

    public class UpdateProductRequest : CreateProductRequest
    {
        public Guid Id { get; set; }
    }
}