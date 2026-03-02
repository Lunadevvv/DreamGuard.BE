using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamGuard.BE.DAL.Configurations
{
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.ToTable("CartItems");
            builder.HasKey(ci => ci.Id);

            builder.Property(ci => ci.Quantity).IsRequired();
            builder.Property(ci => ci.CreatedAt).HasColumnType("TIMESTAMPTZ");
            builder.Property(ci => ci.UpdatedAt).HasColumnType("TIMESTAMPTZ");

            builder.HasOne(ci => ci.ProductVariant)
                .WithMany()
                .HasForeignKey(ci => ci.ProductVariantId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(ci => ci.Combo)
                .WithMany()
                .HasForeignKey(ci => ci.ComboId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_CartItem_ProductOrCombo",
                "(\"ProductVariantId\" IS NOT NULL AND \"ComboId\" IS NULL) OR (\"ProductVariantId\" IS NULL AND \"ComboId\" IS NOT NULL)"
            ));

            builder.HasIndex(ci => new { ci.CartId, ci.ProductVariantId })
                .IsUnique()
                .HasFilter("\"ProductVariantId\" IS NOT NULL");

            builder.HasIndex(ci => new { ci.CartId, ci.ComboId })
                .IsUnique()
                .HasFilter("\"ComboId\" IS NOT NULL");
        }
    }
}
