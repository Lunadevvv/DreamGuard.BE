using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            builder.Property(e => e.BasePrice).IsRequired();
            builder.Property(e => e.SalePrice).IsRequired();
            builder.Property(e => e.CreatedAt).HasColumnType("TIMESTAMPTZ");
            builder.Property(e => e.Attributes).HasColumnType("JSONB");
            builder.HasOne(p => p.Product)
                .WithMany(v => v.Variants)
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.ToTable("ProductVariants");
        }
    }
}