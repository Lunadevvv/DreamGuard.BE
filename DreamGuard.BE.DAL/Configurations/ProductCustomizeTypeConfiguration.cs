using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamGuard.BE.DAL.Configurations
{
    public class ProductCustomizeTypeConfiguration : IEntityTypeConfiguration<ProductCustomizeType>
    {
        public void Configure(EntityTypeBuilder<ProductCustomizeType> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.DefaultPrice).HasColumnType("decimal(18,2)");
            builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            builder.ToTable("ProductCustomizeTypes");
        }
    }
}