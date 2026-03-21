using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class ProductType
    {
        public Guid ProductTypeId { get; set; } = Guid.NewGuid();
        public string ProductTypeName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public decimal AddPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<ServicePackageMapping> ServicePackageMappings { get; set; } = new List<ServicePackageMapping>();
    }
}
