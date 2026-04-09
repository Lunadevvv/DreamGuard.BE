namespace DreamGuard.BE.DAL.Models
{
    public class Customer
    {
        public Guid CustomerId { get; set; } = Guid.NewGuid();
        public string FullName { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public int MemberCoin { get; set; } = 0;
        public User User { get; set; } = null!;
        public ICollection<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();
        public ICollection<BabyProfile> BabyProfiles { get; set; } = new List<BabyProfile>();
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        public ICollection<UserVoucher> UserVouchers { get; set; } = new List<UserVoucher>();
        public Cart? Cart { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<FavoriteProduct> FavoriteProducts { get; set; } = new List<FavoriteProduct>();
        public ICollection<ProductFeedback> ProductFeedbacks { get; set; } = new List<ProductFeedback>();
    }
}
