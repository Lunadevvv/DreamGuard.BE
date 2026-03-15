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

        public string CustomNote { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;    

        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public DateTime AppointmentDate { get; set; } = DateTime.UtcNow.AddDays(3); // Default appointment date is 3 days from now, staff contact customer to confirm exact date 

        public string Status { get; set; } = string.Empty;

        public decimal TotalPrice { get; set; }

        public DateTime CreatedAt { get ; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = null;
        public Customer Customer { get; set; } = null!;
        public ServicePackageMapping ServicePackageMapping { get; set; } = null!;
        public ServiceTask? ServiceTask { get; set; } = null;
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
