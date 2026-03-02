using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamGuard.BE.DAL.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");
            builder.HasKey(o => o.Id);

            builder.Property(o => o.ReceiverName).IsRequired().HasMaxLength(200);
            builder.Property(o => o.PhoneNumber).IsRequired().HasMaxLength(20);
            builder.Property(o => o.Street).IsRequired().HasMaxLength(500);
            builder.Property(o => o.City).IsRequired().HasMaxLength(100);
            builder.Property(o => o.District).IsRequired().HasMaxLength(100);
            builder.Property(o => o.Ward).IsRequired().HasMaxLength(100);
            builder.Property(o => o.Province).IsRequired().HasMaxLength(100);
            builder.Property(o => o.Note).HasMaxLength(500);

            builder.Property(o => o.SubTotal).HasColumnType("decimal(18,2)");
            builder.Property(o => o.DiscountAmount).HasColumnType("decimal(18,2)");
            builder.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");

            builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
            builder.Property(o => o.PaymentMethod).HasConversion<string>().HasMaxLength(20);
            builder.Property(o => o.PaymentStatus).HasConversion<string>().HasMaxLength(20);

            builder.Property(o => o.CreatedAt).HasColumnType("TIMESTAMPTZ");
            builder.Property(o => o.UpdatedAt).HasColumnType("TIMESTAMPTZ");

            builder.HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(o => o.UserVoucher)
                .WithMany()
                .HasForeignKey(o => o.UserVoucherId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(o => o.UserId);
            builder.HasIndex(o => o.Status);
        }
    }
}
