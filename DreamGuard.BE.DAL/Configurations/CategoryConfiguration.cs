using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamGuard.BE.DAL.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(c => c.CateId);
            builder.Property(c => c.CateId)
                .ValueGeneratedOnAdd();
            builder.Property(c => c.Name).IsRequired().HasMaxLength(255);
            builder.Property(c => c.slug).IsRequired().HasMaxLength(255);
            builder.HasOne(c => c.CateParent)
                .WithMany(c => c.ChildCategoryList)
                .HasForeignKey(c => c.CateParentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}