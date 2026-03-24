using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.ModelExtensions
{
    public class ProductCustomizeDetail
    {
        public string CustomizeTypeName { get; set; } = string.Empty;
        public string CustomizeContent { get; set; } = string.Empty;
        public decimal AddOnPrice { get; set; }
    }
}