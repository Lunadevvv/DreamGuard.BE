using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamGuard.BE.DAL.Configurations
{
    public class VariantCustomizeTypeConfiguration : IEntityTypeConfiguration<VariantCustomizeType>
    {
        public void Configure(EntityTypeBuilder<VariantCustomizeType> builder)
        {
            builder.HasKey(e => new { e.CusId, e.ProductVariantId });
            builder.Property(e => e.OverridePrice).HasColumnType("decimal(18,2)");

            builder.HasOne(vct => vct.ProductVariant)
                .WithMany(pv => pv.VariantCustomizeTypes)
                .HasForeignKey(vct => vct.ProductVariantId);
                
            builder.HasOne(vct => vct.ProductCustomizeType)
                .WithMany(pct => pct.VariantCustomizeTypes)
                .HasForeignKey(vct => vct.CusId);
            builder.ToTable("VariantCustomizeTypes");
        }
    }
}