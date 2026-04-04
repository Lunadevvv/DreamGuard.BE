using System;
using System.Collections.Generic;

namespace DreamGuard.BE.BLL.Requests
{
    public class ProcessReturnedRequest
    {
        public string DamageNote { get; set; } = string.Empty;
        public List<string> EvidenceUrls { get; set; } = new List<string>();
        public List<DamagedItemRequest> DamagedItems { get; set; } = new List<DamagedItemRequest>();
    }

    public class DamagedItemRequest
    {
        public Guid OrderItemId { get; set; }
        public int DamagedQuantity { get; set; }
    }
}
