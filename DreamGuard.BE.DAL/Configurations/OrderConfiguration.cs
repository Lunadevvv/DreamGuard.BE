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

            builder.Property(o => o.OrderCode).IsRequired().HasMaxLength(50);
            builder.HasIndex(o => o.OrderCode).IsUnique();

            builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);

            builder.Property(o => o.ReceiverName).IsRequired().HasMaxLength(200);
            builder.Property(o => o.PhoneNumber).IsRequired().HasMaxLength(20);
            builder.Property(o => o.Street).HasMaxLength(500);
            builder.Property(o => o.City).HasMaxLength(100);
            builder.Property(o => o.District).HasMaxLength(100);
            builder.Property(o => o.Ward).HasMaxLength(100);
            builder.Property(o => o.Province).HasMaxLength(100);

            builder.Property(o => o.SubTotal).IsRequired();
            builder.Property(o => o.DiscountAmount).IsRequired();
            builder.Property(o => o.TotalAmount).IsRequired();

            builder.Property(o => o.Note).HasMaxLength(500);
            builder.Property(o => o.CreatedAt).HasColumnType("TIMESTAMPTZ");
            builder.Property(o => o.UpdatedAt).HasColumnType("TIMESTAMPTZ");

            builder.HasOne(o => o.UserVoucher)
                .WithMany()
                .HasForeignKey(o => o.UserVoucherId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(o => o.CheckoutProductOrder)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CheckoutProductOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(o => o.CustomerId);
            builder.HasIndex(o => o.Status);
        }
    }
}
