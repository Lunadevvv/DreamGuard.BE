using System.Collections.Generic;

namespace DreamGuard.BE.BLL.Requests
{
    public class StartShippingRequest
    {
        public List<string> EvidenceUrls { get; set; } = new List<string>();
    }
}
