using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DreamGuard.BE.BLL.Requests
{
    public class CompleteShippingRequest
    {
        public List<string> EvidenceUrls { get; set; } = new List<string>();
        public string? PaymentEvidenceUrl { get; set; }
    }
    public class CompleteShippingForTradeInRequest
    {
        public List<string> EvidenceUrls { get; set; } = new List<string>();
    }
}
