using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class Otp
    {
        public Guid OtpId { get; set; }
        public required string Phone { get; set; }
        public required string Email { get; set; }
        public required string CodeHashed { get; set; }
        public required byte[] Salt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiredAt { get; set; }
        public bool IsUsed { get; set; } = false;
    }
}