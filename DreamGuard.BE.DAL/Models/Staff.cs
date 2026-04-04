using DreamGuard.BE.DAL.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class Staff
    {
        public Guid StaffId { get; set; } 
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string Position { get; set; } = Role.CleaningStaff;
        public string Gender { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string Status { get; set; } = StaffStatus.Active;
        public double AverageRating { get; set; } = 0.0;
        public int TotalRating { get; set; } = 0;
        public User User { get; set; } = null!;
        public ICollection<ServiceTask> ServiceTasks { get; set; } = new List<ServiceTask>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public ICollection<ShippingTask> ShippingTasks { get; set; } = new List<ShippingTask>();
    }
}
