using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class ServiceOrderItem
    {
        public Guid ServiceOrderItemId { get; set; } = Guid.NewGuid();
        public Guid SoId { get; set; }

        public Guid ServicePackageMappingId { get; set; }

        public decimal TotalPrice { get; set; }
        public int Quantity { get; set; }
        public ServicePackageMapping ServicePackageMapping { get; set; } = null!;
        public ServiceOrder ServiceOrder { get; set; } = null!;
    }
}
