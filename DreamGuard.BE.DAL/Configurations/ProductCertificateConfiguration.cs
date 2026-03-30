using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamGuard.BE.DAL.Configurations
{
    public class ProductCertificateConfiguration : IEntityTypeConfiguration<ProductCertificate>
    {
        public void Configure(EntityTypeBuilder<ProductCertificate> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
            builder.Property(e => e.Summary).HasMaxLength(500);
            builder.Property(e => e.Description).HasColumnType("TEXT");
            builder.ToTable("ProductCertificates");

            builder.HasMany(pc => pc.Products)
                .WithMany(p => p.Certificates)
                .UsingEntity<Dictionary<string, Guid>>(
                    "ProductProductCertificate",
                    j => j
                        .HasOne<Product>()
                        .WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<ProductCertificate>()
                        .WithMany()
                        .HasForeignKey("ProductCertificateId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("ProductId", "ProductCertificateId");
                        j.ToTable("ProductProductCertificates");
                    });
        }
    }
}