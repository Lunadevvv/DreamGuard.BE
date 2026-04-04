using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DreamGuard.BE.BLL.Requests
{
    public class FailShippingRequest
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
        
        public List<string> EvidenceUrls { get; set; } = new List<string>();
    }
}
