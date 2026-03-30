using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class Rating
    {
        public Guid RatingId { get; set; } = Guid.NewGuid();
        public Guid StaffId { get; set; }
        public Guid ServiceOrderId { get; set; }
        public int Score { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public Staff Staff { get; set; } = null!;
        public ServiceOrder ServiceOrder { get; set; } = null!;
    }
}
