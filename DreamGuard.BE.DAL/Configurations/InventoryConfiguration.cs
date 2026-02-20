using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamGuard.BE.DAL.Configurations
{
    public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
    {
        public void Configure(EntityTypeBuilder<Inventory> builder)
        {
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Quantity).IsRequired();
            builder.Property(i => i.UpdatedAt).IsRequired();
            builder.Property(i => i.UpdatedAt).HasColumnType("TIMESTAMPTZ");
            builder.HasOne(i => i.ProductVariant)
                .WithOne(pv => pv.Inventory)
                .HasForeignKey<Inventory>(i => i.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("Inventories");
        }
    }
}