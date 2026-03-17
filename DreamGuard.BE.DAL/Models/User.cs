using Microsoft.AspNetCore.Identity;

namespace DreamGuard.BE.DAL.Models
{
    public class User : IdentityUser<Guid>
    {
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRevoked { get; set; } = false;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Customer? Customer { get; set; }
        public Staff? Staff { get; set; }
    }
}
