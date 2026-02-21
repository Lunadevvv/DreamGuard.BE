using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class ProductAssetResponse
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
    }
}