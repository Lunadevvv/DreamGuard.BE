using System.Text.Json;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

            builder.Property(oi => oi.ProductCustomizeDetails)
                .HasColumnType("jsonb") // Hoặc tương tự theo database
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    // Kiểm tra string rỗng hoặc null thay vì gọi Deserialize trực tiếp
                    v => (string.IsNullOrWhiteSpace(v) || v == "null" || v == "\"\"") 
                            ? new List<ProductCustomizeDetail>() 
                            : JsonSerializer.Deserialize<List<ProductCustomizeDetail>>(v, (JsonSerializerOptions?)null)
                )
                .Metadata.SetValueComparer(new ValueComparer<List<ProductCustomizeDetail>>(
                    (c1, c2) => JsonSerializer.Serialize(c1, (JsonSerializerOptions)null) == JsonSerializer.Serialize(c2, (JsonSerializerOptions)null),
                    c => c == null ? 0 : JsonSerializer.Serialize(c, (JsonSerializerOptions)null).GetHashCode(),
                    // Ở snapshot này cũng cần an toàn
                    c => c == null 
                            ? new List<ProductCustomizeDetail>() 
                            : JsonSerializer.Deserialize<List<ProductCustomizeDetail>>(JsonSerializer.Serialize(c, (JsonSerializerOptions)null), (JsonSerializerOptions)null)
                ));

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
