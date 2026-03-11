using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class ServiceAsset
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ServiceId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Service Service { get; set; } = null!;

    }
}
