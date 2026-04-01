using System.Text.Json;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

            builder.Property(oi => oi.CustomizeHash)
                .HasMaxLength(100);

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
