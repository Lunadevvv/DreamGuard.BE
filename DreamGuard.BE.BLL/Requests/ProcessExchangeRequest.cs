using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DreamGuard.BE.BLL.Requests
{
    public class ProcessExchangeRequest
    {
        [Required]
        public Guid NewStaffId { get; set; }

        public string ExchangeNote { get; set; } = string.Empty;

        public List<string> EvidenceUrls { get; set; } = new List<string>();

        public List<DamagedItemRequest> DamagedItems { get; set; } = new List<DamagedItemRequest>();
    }
}
