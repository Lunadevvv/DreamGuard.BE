using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class ServiceTaskCheckInRequest
    {
        [MinLength(1, ErrorMessage = "At least one evidence URL is required.")]
        public List<string> EvidenceUrls { get; set; } = new List<string>();
    }
    public class ServiceTaskCheckOutRequest
    {
        [MinLength(1, ErrorMessage = "At least one evidence URL is required.")]
        public List<string> EvidenceUrls { get; set; } = new List<string>();
    }
    public class ServiceTaskCompleteRequest
    {
        public string EvidenceUrl { get; set; } = string.Empty;
    }
}
