using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.DbContext
{
    public class DreamGuardContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public DreamGuardContext()
        {
        }

        public DreamGuardContext(DbContextOptions<DreamGuardContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Otp> Otps { get; set; }
        public DbSet<BabyProfile> BabyProfiles { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductAsset> ProductAssets { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Combo> Combos { get; set; }
        public DbSet<ComboProductVariant> ComboProductVariants { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<UserVoucher> UserVouchers { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ProductCustomizeType> ProductCustomizeTypes { get; set; }
        public DbSet<VariantCustomizeType> VariantCustomizeTypes { get; set; }
        public DbSet<ProductCertificate> ProductCertificates { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<FavoriteProduct> FavoriteProducts { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<ServiceOrder> ServiceOrders { get; set; }
        public DbSet<ServiceOrderItem> ServiceOrderItems { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }
        public DbSet<ServicePackage> ServicePackages { get; set; }
        public DbSet<ServicePackageMapping> ServicePackageMappings { get; set; }
        public DbSet<ServiceTask> ServiceTasks { get; set; }
        public DbSet<ServiceEvidence> ServiceEvidences { get; set; }
        public DbSet<ServiceAsset> ServiceAssets { get; set; }
        public DbSet<Rating> Ratings { get; set; }  
        public DbSet<ShippingTask> ShippingTasks { get; set; }
        public DbSet<ShippingEvidence> ShippingEvidences { get; set; }
        public DbSet<TradeInOrder> TradeInOrders { get; set; }
        public DbSet<TradeInImage> TradeInImages { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // viết configuration cho các entity qua class riêng và tự động apply tất cả các configuration trong assembly ( đỡ rối & dễ sửa)
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

    }
}
