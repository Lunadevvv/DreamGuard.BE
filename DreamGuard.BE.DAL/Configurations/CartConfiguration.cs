using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamGuard.BE.DAL.Configurations
{
    public class CartConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.ToTable("Carts");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.CreatedAt).HasColumnType("TIMESTAMPTZ");
            builder.Property(c => c.UpdatedAt).HasColumnType("TIMESTAMPTZ");

            builder.HasOne(c => c.Customer)
                .WithOne(cust => cust.Cart)
                .HasForeignKey<Cart>(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(c => c.CustomerId).IsUnique();
        }
    }
}
