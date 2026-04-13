using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class ProcessExchangeTradeInRequest
    {
        [Required]
        public Guid NewStaffId { get; set; }

        public string ExchangeNote { get; set; } = string.Empty;

        public List<string> EvidenceUrls { get; set; } = new List<string>();

        public Guid ProductVariantId { get; set; }
    }
}
