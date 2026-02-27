using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class BabyProfile
    {
        public Guid BabyId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public double Weight { get; set; } 
        public double Height { get; set; }
        public string Note { get; set; } = string.Empty;

        public User User { get; set; } = null!;
    }
}
