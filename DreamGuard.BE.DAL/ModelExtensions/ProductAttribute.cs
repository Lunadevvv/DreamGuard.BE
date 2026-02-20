using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.ModelExtensions
{
    public class ProductAttribute
    {
        public double? Width { get; set; }
        public double? Length { get; set; }
        public double? Thickness { get; set; }
        public double? Weight { get; set; }
        public string? Color { get; set; }
    }
}