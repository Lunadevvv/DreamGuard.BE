using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class ServicePackageMapping
    {
        public Guid ServicePackageMappingId { get; set; }
        public Guid ServiceId { get; set; }
        public Guid ServicePackageId { get; set; }
        public int Duration { get; set; } // Duration in minutes
        public decimal Price { get; set; }
        public Service Service { get; set; } = null!;
        public ICollection<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();
        public ServicePackage ServicePackage { get; set; } = null!;
    }
}
