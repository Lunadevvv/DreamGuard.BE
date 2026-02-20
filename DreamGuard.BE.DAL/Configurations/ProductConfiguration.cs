using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamGuard.BE.DAL.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
            builder.Property(e => e.Slug).IsRequired().HasMaxLength(255);
            builder.Property(e => e.Summary).HasMaxLength(500);
            builder.Property(e => e.Description).HasColumnType("TEXT");
            builder.Property(e => e.AgeGroup).HasMaxLength(100);
            builder.Property(e => e.CreatedAt).HasColumnType("TIMESTAMPTZ");
            builder.ToTable("Products");
            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(p => p.Slug).IsUnique();
        }
    }
}