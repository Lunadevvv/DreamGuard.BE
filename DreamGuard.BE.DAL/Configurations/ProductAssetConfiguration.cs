using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamGuard.BE.DAL.Configurations
{
    public class ProductAssetConfiguration : IEntityTypeConfiguration<ProductAsset>
    {
        public void Configure(EntityTypeBuilder<ProductAsset> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Url).IsRequired().HasMaxLength(500);
            builder.Property(e => e.Type).IsRequired().HasMaxLength(100);
            builder.Property(e => e.PublicId).IsRequired().HasMaxLength(255);
            builder.HasOne(p => p.Product)
                .WithMany(v => v.Assets)
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.ToTable("ProductAssets");
        }
    }
}