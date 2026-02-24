using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.BLL.Requests
{
    public class ComboItemRequest
    {
        [Required(ErrorMessage = "ProductVariantId is required.")]
        public Guid ProductVariantId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }

    public class CreateComboRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(200, ErrorMessage = "Name must not exceed 200 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug is required.")]
        [MaxLength(200, ErrorMessage = "Slug must not exceed 200 characters.")]
        public string Slug { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "AgeGroup must be a non-negative value.")]
        public int? AgeGroup { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        [Required(ErrorMessage = "BasePrice is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "BasePrice must be a non-negative value.")]
        public double BasePrice { get; set; }

        [Required(ErrorMessage = "SalePrice is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "SalePrice must be a non-negative value.")]
        public double SalePrice { get; set; }

        public string Description { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public string ImagePublicId { get; set; } = string.Empty;

        public Guid? ComboParentId { get; set; }

        public List<ComboItemRequest>? Items { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<CreateComboRequest, Combo>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.ComboProductVariants, opt => opt.Ignore())
                    .ForMember(dest => dest.ComboParent, opt => opt.Ignore())
                    .ForMember(dest => dest.ComboChildrens, opt => opt.Ignore())
                    .ForMember(dest => dest.AverageRating, opt => opt.Ignore())
                    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                    .ForMember(dest => dest.IsActive, opt => opt.Ignore());
            }
        }
    }

    public class UpdateComboInfoRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(200, ErrorMessage = "Name must not exceed 200 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug is required.")]
        [MaxLength(200, ErrorMessage = "Slug must not exceed 200 characters.")]
        public string Slug { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "AgeGroup must be a non-negative value.")]
        public int? AgeGroup { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        [Required(ErrorMessage = "BasePrice is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "BasePrice must be a non-negative value.")]
        public double BasePrice { get; set; }

        [Required(ErrorMessage = "SalePrice is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "SalePrice must be a non-negative value.")]
        public double SalePrice { get; set; }

        public string Description { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public string ImagePublicId { get; set; } = string.Empty;

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<UpdateComboInfoRequest, Combo>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.ComboParentId, opt => opt.Ignore())
                    .ForMember(dest => dest.ComboProductVariants, opt => opt.Ignore())
                    .ForMember(dest => dest.ComboParent, opt => opt.Ignore())
                    .ForMember(dest => dest.ComboChildrens, opt => opt.Ignore())
                    .ForMember(dest => dest.AverageRating, opt => opt.Ignore())
                    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                    .ForMember(dest => dest.IsActive, opt => opt.Ignore());
            }
        }
    }

    public class UpdateComboProductsRequest
    {
        [Required(ErrorMessage = "Items list is required.")]
        [MinLength(1, ErrorMessage = "At least one item is required.")]
        public List<ComboItemRequest> Items { get; set; } = new();
    }
}
