using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamGuard.BE.DAL.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.CreatedAt).HasColumnType("TIMESTAMPTZ");
            builder.Property(e => e.UpdatedAt).HasColumnType("TIMESTAMPTZ");
            builder.ToTable("Users");
        }
    }
}
