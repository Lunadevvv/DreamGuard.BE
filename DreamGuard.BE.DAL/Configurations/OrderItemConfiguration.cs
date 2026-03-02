using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamGuard.BE.DAL.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");
            builder.HasKey(oi => oi.Id);

            builder.Property(oi => oi.ProductName).IsRequired().HasMaxLength(300);
            builder.Property(oi => oi.Sku).HasMaxLength(100);
            builder.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
            builder.Property(oi => oi.TotalPrice).HasColumnType("decimal(18,2)");
            builder.Property(oi => oi.Quantity).IsRequired();

            builder.HasOne(oi => oi.ProductVariant)
                .WithMany()
                .HasForeignKey(oi => oi.ProductVariantId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(oi => oi.Combo)
                .WithMany()
                .HasForeignKey(oi => oi.ComboId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_OrderItem_ProductOrCombo",
                "(\"ProductVariantId\" IS NOT NULL AND \"ComboId\" IS NULL) OR (\"ProductVariantId\" IS NULL AND \"ComboId\" IS NOT NULL)"
            ));
        }
    }
}
