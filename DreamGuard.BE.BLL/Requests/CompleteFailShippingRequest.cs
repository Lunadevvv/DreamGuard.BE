using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DreamGuard.BE.BLL.Requests
{
    public class CompleteFailShippingRequest
    {
        [Required]
        public List<string> EvidenceUrls { get; set; } = new List<string>();
    }
}
