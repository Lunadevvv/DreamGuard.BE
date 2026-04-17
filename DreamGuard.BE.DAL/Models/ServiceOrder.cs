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
        public Guid? UserVoucherId { get; set; }
        public Guid CustomerId { get; set; }

        public string OrderCode { get; set; } = string.Empty;

        public string CustomerNote { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;    

        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        public DateTime AppointmentDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public decimal TotalPrice { get; set; }
        public decimal SubTotalPrice { get; set; }

        public DateTime CreatedAt { get ; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = null;
        public Customer Customer { get; set; } = null!;
        public ICollection<ServiceTask> ServiceTasks { get; set; } = new List<ServiceTask>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<ServiceOrderItem> ServiceOrderItems { get; set; } = new List<ServiceOrderItem>();
        public ICollection<ServiceAsset> ServiceAssets { get; set; } = new List<ServiceAsset>();
        public UserVoucher? UserVoucher { get; set; } 
        public Rating? Rating { get; set; }
    }
}
