using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamGuard.BE.DAL.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.OrderCode).IsRequired().HasMaxLength(50);
            builder.HasIndex(p => p.OrderCode);

            builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
            builder.Property(p => p.PaymentMethod).HasConversion<string>().HasMaxLength(20);

            builder.Property(p => p.Amount).IsRequired();
            builder.Property(p => p.Description).HasMaxLength(500);

            builder.Property(p => p.CreatedAt).HasColumnType("TIMESTAMPTZ");
            builder.Property(p => p.UpdatedAt).HasColumnType("TIMESTAMPTZ");
            builder.Property(p => p.ExpiredAt).HasColumnType("TIMESTAMPTZ");

            builder.HasOne(p => p.POrder)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.POrderId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(p => p.POrderId);
            builder.HasIndex(p => p.Status);
        }
    }
}
