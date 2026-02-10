using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.DAL.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.FirstName).HasMaxLength(10);
            builder.Property(e => e.LastName).HasMaxLength(10);
            builder.Property(e => e.DateOfBirth).IsRequired();
            builder.Property(e => e.CreatedAt).HasColumnType("TIMESTAMPTZ");
            builder.Property(e => e.UpdatedAt).HasColumnType("TIMESTAMPTZ");
            builder.ToTable("Users");
        }
    }
}
