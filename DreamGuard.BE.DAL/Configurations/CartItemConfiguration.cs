using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamGuard.BE.DAL.Configurations
{
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.ToTable("CartItems", t =>
            {
                t.HasCheckConstraint("CK_CartItem_ItemType",
                    "(\"ProductVariantId\" IS NOT NULL AND \"ComboId\" IS NULL) OR (\"ProductVariantId\" IS NULL AND \"ComboId\" IS NOT NULL)");
            });
            builder.HasKey(ci => ci.Id);

            builder.Property(ci => ci.Quantity).IsRequired();
            builder.Property(ci => ci.AddedAt).HasColumnType("TIMESTAMPTZ");

            builder.HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ci => ci.ProductVariant)
                .WithMany()
                .HasForeignKey(ci => ci.ProductVariantId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(ci => ci.Combo)
                .WithMany()
                .HasForeignKey(ci => ci.ComboId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
