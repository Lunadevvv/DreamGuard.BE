using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamGuard.BE.DAL.Configurations
{
    public class ComboConfiguration : IEntityTypeConfiguration<Combo>
    {
        public void Configure(EntityTypeBuilder<Combo> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
            builder.Property(c => c.Slug).IsRequired().HasMaxLength(200);
            builder.Property(c => c.Size).HasMaxLength(20);
            builder.Property(c => c.Color).HasMaxLength(50);
            builder.Property(c => c.CreatedAt).HasColumnType("TIMESTAMPTZ");
            builder.Property(c => c.Description).HasColumnType("text");
            builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

            builder.HasOne(c => c.ComboParent)
                    .WithMany(cp => cp.ComboChildrens)
                    .HasForeignKey(cp => cp.ComboParentId)
                    .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasIndex(c => c.Slug).IsUnique();
        }
    }
}