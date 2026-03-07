using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamGuard.BE.DAL.Configurations
{
    public class FavoriteProductConfiguration : IEntityTypeConfiguration<FavoriteProduct>
    {
        public void Configure(EntityTypeBuilder<FavoriteProduct> builder)
        {
            builder.ToTable("FavoriteProducts");
            builder.HasKey(fp => fp.Id);

            builder.Property(fp => fp.CreatedAt).HasColumnType("TIMESTAMPTZ");

            builder.HasOne(fp => fp.User)
                .WithMany(u => u.FavoriteProducts)
                .HasForeignKey(fp => fp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(fp => fp.Product)
                .WithMany()
                .HasForeignKey(fp => fp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(fp => new { fp.UserId, fp.ProductId }).IsUnique();
        }
    }
}
