using DreamGuard.BE.DAL.Constants;
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
        public string PackageName { get; set; } = string.Empty;
        public ServicePackageStatus Status { get; set; } = ServicePackageStatus.Draft;
        public int Duration { get; set; } = 0; // in minutes
        public string SuitableFor { get; set; } = string.Empty;
        public string ServiceContent { get; set; } = string.Empty;
        public string Benefits { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public ICollection<ServicePackageMapping> ServicePackageMappings { get; set; } = new List<ServicePackageMapping>();

    }
}
