using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class ServicePackage
    {
        public Guid ServicePackageId { get; set; } = Guid.NewGuid();
        public Guid ServiceId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
        public int Duration { get; set; } = 0; // in minutes
        public Service Service { get; set; } = null!;

    }
}
