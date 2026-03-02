using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamGuard.BE.DAL.Configurations
{
    public class ComboProductVariantConfiguration : IEntityTypeConfiguration<ComboProductVariant>
    {
        public void Configure(EntityTypeBuilder<ComboProductVariant> builder)
        {
            builder.HasKey(cp => cp.Id);

            builder.HasOne(cp => cp.Combo)
                .WithMany(c => c.ComboProductVariants)
                .HasForeignKey(cp => cp.ComboId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cp => cp.ProductVariant)
                .WithMany(cp => cp.ComboProductVariants)
                .HasForeignKey(cp => cp.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}