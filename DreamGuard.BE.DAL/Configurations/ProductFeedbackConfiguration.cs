using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamGuard.BE.DAL.Configurations
{
    public class ProductFeedbackConfiguration : IEntityTypeConfiguration<ProductFeedback>
    {
        public void Configure(EntityTypeBuilder<ProductFeedback> builder)
        {
            builder.HasKey(f => f.Id);
            builder.ToTable("ProductFeedbacks");

            builder.Property(f => f.Score).IsRequired();
            builder.Property(f => f.Comment).HasColumnType("TEXT");
            builder.Property(f => f.Status).IsRequired().HasMaxLength(20);
            builder.Property(f => f.CreatedAt).HasColumnType("TIMESTAMPTZ");
            builder.Property(f => f.UpdatedAt).HasColumnType("TIMESTAMPTZ");

            // Unique: one feedback per customer per product per order
            builder.HasIndex(f => new { f.ProductId, f.OrderId, f.CustomerId }).IsUnique();

            // n-1 ProductFeedback - Product
            builder.HasOne(f => f.Product)
                .WithMany(p => p.ProductFeedbacks)
                .HasForeignKey(f => f.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // n-1 ProductFeedback - Order
            builder.HasOne(f => f.Order)
                .WithMany(o => o.ProductFeedbacks)
                .HasForeignKey(f => f.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // n-1 ProductFeedback - Customer
            builder.HasOne(f => f.Customer)
                .WithMany(c => c.ProductFeedbacks)
                .HasForeignKey(f => f.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
