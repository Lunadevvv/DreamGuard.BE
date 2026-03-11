using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Models
{
    public class User : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRevoked { get; set; } = false;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<BabyProfile> BabyProfiles { get; set; } = new List<BabyProfile>();
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        public ICollection<UserVoucher> UserVouchers { get; set; } = new List<UserVoucher>();
        public Cart? Cart { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<FavoriteProduct> FavoriteProducts { get; set; } = new List<FavoriteProduct>();

    }
}
