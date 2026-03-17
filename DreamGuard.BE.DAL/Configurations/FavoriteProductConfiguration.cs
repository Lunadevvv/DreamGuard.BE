using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamGuard.BE.DAL.Configurations
{
    public class FavoriteProductConfiguration : IEntityTypeConfiguration<FavoriteProduct>
    {
        public void Configure(EntityTypeBuilder<FavoriteProduct> builder)
        {
            builder.ToTable("FavoriteProducts", t => t.HasCheckConstraint("CK_FavoriteProduct_ProductOrCombo", 
                "(\"ProductId\" IS NOT NULL AND \"ComboId\" IS NULL) OR (\"ProductId\" IS NULL AND \"ComboId\" IS NOT NULL)"));
            builder.HasKey(fp => fp.Id);

            builder.Property(fp => fp.CreatedAt).HasColumnType("TIMESTAMPTZ");

            builder.HasOne(fp => fp.Product)
                .WithMany()
                .HasForeignKey(fp => fp.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            builder.HasOne(fp => fp.Combo)
                .WithMany()
                .HasForeignKey(fp => fp.ComboId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            builder.HasIndex(fp => new { fp.CustomerId, fp.ProductId })
                .IsUnique()
                .HasFilter("\"ProductId\" IS NOT NULL");

            builder.HasIndex(fp => new { fp.CustomerId, fp.ComboId })
                .IsUnique()
                .HasFilter("\"ComboId\" IS NOT NULL");
        }
    }
}
