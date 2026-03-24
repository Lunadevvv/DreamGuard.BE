using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.BLL.Requests
{
    public class CreateProductVariantRequest
    {
        [MaxLength(100, ErrorMessage = "SKU must not exceed 100 characters.")]
        public string? Sku { get; set; }

        [Required(ErrorMessage = "Base price is required.")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Base price must be a non-negative value.")]
        public decimal BasePrice { get; set; }

        [Required(ErrorMessage = "Sale price is required.")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Sale price must be a non-negative value.")]
        public decimal SalePrice { get; set; }

        [Range(0, 1000, ErrorMessage = "Weight must be between 0 and 1000.")]
        public double? Weight { get; set; }

        public ProductAttribute? Attributes { get; set; }

        public bool IsNew { get; set; } = true;

        [Required(ErrorMessage = "Product ID is required.")]
        public Guid ProductId { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<CreateProductVariantRequest, ProductVariant>();
            }
        }
    }

    public class UpdateProductVariantRequest
    {
        [MaxLength(100, ErrorMessage = "SKU must not exceed 100 characters.")]
        public string? Sku { get; set; }

        [Required(ErrorMessage = "Base price is required.")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Base price must be a non-negative value.")]
        public decimal BasePrice { get; set; }

        [Required(ErrorMessage = "Sale price is required.")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Sale price must be a non-negative value.")]
        public decimal SalePrice { get; set; }

        [Range(0, 1000, ErrorMessage = "Weight must be between 0 and 1000.")]
        public double? Weight { get; set; }

        public ProductAttribute? Attributes { get; set; }

        public bool IsNew { get; set; } = true;

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<UpdateProductVariantRequest, ProductVariant>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                    .ForMember(dest => dest.Status, opt => opt.Ignore())
                    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
            }
        }
    }

    public class CreateVariantWithCustomizeRequest : CreateProductVariantRequest 
    {
        //Customize Types
        public List<Guid> CustomizeTypeIds { get; set; } = new List<Guid>();

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<CreateVariantWithCustomizeRequest, ProductVariant>();
            }
        }
    }

    public class AssignCustomizeTypeRequest
    {
        [Required(ErrorMessage = "CustomizeTypeId is required.")]
        public Guid CustomizeTypeId { get; set; }

        [Range(0, (double)decimal.MaxValue, ErrorMessage = "OverridePrice must be a non-negative value.")]
        public decimal OverridePrice { get; set; }
    }

    public class UpdateCustomizeTypePriceRequest
    {
        [Required(ErrorMessage = "OverridePrice is required.")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "OverridePrice must be a non-negative value.")]
        public decimal OverridePrice { get; set; }
    }
}
