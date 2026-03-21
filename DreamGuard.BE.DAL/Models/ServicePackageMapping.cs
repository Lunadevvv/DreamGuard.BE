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
        public Guid ProductTypeId { get; set; }
        public Guid ServicePackageId { get; set; }
        public int Duration { get; set; } // Duration in minutes
        public decimal Price { get; set; }
        public ProductType ProductType { get; set; } = null!;
        public ICollection<ServiceOrderItem> ServiceOrderItems { get; set; } = new List<ServiceOrderItem>();
        public ServicePackage ServicePackage { get; set; } = null!;
    }
}
