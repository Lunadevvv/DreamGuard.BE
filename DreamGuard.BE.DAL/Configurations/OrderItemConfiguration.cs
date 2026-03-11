using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamGuard.BE.DAL.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems", t =>
            {
                t.HasCheckConstraint("CK_OrderItem_ItemType",
                    "(\"ProductVariantId\" IS NOT NULL AND \"ComboId\" IS NULL) OR (\"ProductVariantId\" IS NULL AND \"ComboId\" IS NOT NULL)");
            });
            builder.HasKey(oi => oi.Id);

            builder.Property(oi => oi.Quantity).IsRequired();
            builder.Property(oi => oi.UnitPrice).IsRequired();
            builder.Property(oi => oi.TotalPrice).IsRequired();
            builder.Property(oi => oi.ItemName).IsRequired().HasMaxLength(300);

            builder.HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(oi => oi.ProductVariant)
                .WithMany()
                .HasForeignKey(oi => oi.ProductVariantId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(oi => oi.Combo)
                .WithMany()
                .HasForeignKey(oi => oi.ComboId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
