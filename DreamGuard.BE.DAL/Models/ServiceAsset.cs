using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class ServiceAsset
    {
        public Guid ServiceAssetId { get; set; } = Guid.NewGuid();
        public Guid ServiceOrderId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        [JsonIgnore]
        public ServiceOrder ServiceOrder { get; set; } = null!;
    }
}
