using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class ServiceOrder
    {
        public Guid SoId { get; set; } = Guid.NewGuid();

        public Guid CustomerId { get; set; }

        public Guid ServicePackageMappingId { get; set; }

        public string OrderCode { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public string CustomNote { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public DateTime AppointmentDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public decimal TotalPrice { get; set; }

        public DateTime CreatedAt { get ; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = null;
        public Customer Customer { get; set; } = null!;
        public ServicePackageMapping ServicePackageMapping { get; set; } = null!;
        public ServiceTask? ServiceTask { get; set; } = null;
    }
}
