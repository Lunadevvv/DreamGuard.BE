using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DreamGuard.BE.BLL.Requests
{
    public class SubmitReturnRequest
    {
        [Required]
        public string Reason { get; set; } = string.Empty;

        public List<DamagedItemRequest> DamagedItems { get; set; } = new List<DamagedItemRequest>();

        [Required]
        public List<string> EvidenceUrls { get; set; } = new List<string>();
    }
}
