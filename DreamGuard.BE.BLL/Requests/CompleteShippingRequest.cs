using System;
using System.Collections.Generic;

namespace DreamGuard.BE.BLL.Requests
{
    public class CompleteShippingRequest
    {
        public List<string> EvidenceUrls { get; set; } = new List<string>();
    }
}
