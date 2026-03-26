using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.BLL.Requests
{
    public class CreateFullyCustomizeProductRequest : Product
    {
        [Required (ErrorMessage = "Fully Customized Product Type is required.")]
        public FullyCustomizedProductType FullyCustomizedProductType { get; set; }
        public string? Sku { get; set; }
        public required decimal BasePrice { get; set; }
        public required decimal SalePrice { get; set; }
        public double? Weight { get; set; }
    }
}