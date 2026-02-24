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


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // viết configuration cho các entity qua class riêng và tự động apply tất cả các configuration trong assembly ( đỡ rối & dễ sửa)
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

    }
}
