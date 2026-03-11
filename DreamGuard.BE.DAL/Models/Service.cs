using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class Service
    {
        public Guid ServiceId { get; set; } = Guid.NewGuid();
        public string ServiceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
        public int EstimatedDuration { get; set; } = 0; // in minutes
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = null;
        public ICollection<ServicePackage> ServicePackages { get; set; } = new List<ServicePackage>();
        public ICollection<ServiceAsset> ServiceAssets { get; set; } = new List<ServiceAsset>();
        public ICollection<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();
    }
}
