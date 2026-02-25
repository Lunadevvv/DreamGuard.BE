using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamGuard.BE.DAL.Configurations
{
    public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Sku).IsRequired().HasMaxLength(100);
            builder.Property(e => e.BasePrice).IsRequired();
            builder.Property(e => e.SalePrice).IsRequired();
            builder.Property(e => e.CreatedAt).HasColumnType("TIMESTAMPTZ");
            builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            builder.Property(e => e.Attributes)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<ProductAttribute>(v, (JsonSerializerOptions?)null)
                );
            builder.HasOne(p => p.Product)
                .WithMany(v => v.Variants)
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(p => p.Attributes).HasMethod("gin");
            builder.ToTable("ProductVariants");
        }
    }
}