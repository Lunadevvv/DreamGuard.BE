using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class ProcessReturnedTradeInRequest
    {
        public string DamageNote { get; set; } = string.Empty;
        public List<string> EvidenceUrls { get; set; } = new List<string>();
        public Guid ProductVariantId { get; set; }
        public bool IsRefund { get; set; } 
    }
}
