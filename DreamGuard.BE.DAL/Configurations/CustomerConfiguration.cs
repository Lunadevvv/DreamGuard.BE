using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamGuard.BE.DAL.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customers");
            builder.HasKey(x => x.CustomerId);

            builder.Property(x => x.FullName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Gender).HasMaxLength(10);
            builder.Property(x => x.AvatarUrl).HasMaxLength(500);

            // 1-1 Customer-User
            builder.HasOne(x => x.User)
                   .WithOne(x => x.Customer)
                   .HasForeignKey<Customer>(x => x.CustomerId)
                   .OnDelete(DeleteBehavior.Cascade);

            // 1-N BabyProfiles
            builder.HasMany(x => x.BabyProfiles)
                   .WithOne(x => x.Customer)
                   .HasForeignKey(x => x.CustomerId)
                   .OnDelete(DeleteBehavior.Cascade);

            // 1-N Addresses
            builder.HasMany(x => x.Addresses)
                   .WithOne(x => x.Customer)
                   .HasForeignKey(x => x.CustomerId)
                   .OnDelete(DeleteBehavior.Cascade);

            // 1-N Orders
            builder.HasMany(x => x.Orders)
                   .WithOne(x => x.Customer)
                   .HasForeignKey(x => x.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict);

            // 1-N FavoriteProducts
            builder.HasMany(x => x.FavoriteProducts)
                   .WithOne(x => x.Customer)
                   .HasForeignKey(x => x.CustomerId)
                   .OnDelete(DeleteBehavior.Cascade);

            // 1-N UserVouchers
            builder.HasMany(x => x.UserVouchers)
                   .WithOne(x => x.Customer)
                   .HasForeignKey(x => x.CustomerId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
