using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.ModelExtensions
{
    public class ProductAttribute
    {
        [Range(0, 300, ErrorMessage = "Width must be between 0 and 300 cm.")]
        public int? Width { get; set; }
        [Range(0, 300, ErrorMessage = "Length must be between 0 and 300 cm.")]
        public int? Length { get; set; }
        [Range(0, 50, ErrorMessage = "Thickness must be between 0 and 50 cm.")]
        public double? Thickness { get; set; }
        [Range(0, 1000, ErrorMessage = "Weight must be between 0 and 1000 kg.")]
        public double? Weight { get; set; }
        public string? Color { get; set; }
    }
}