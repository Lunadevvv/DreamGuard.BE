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
    internal class ServicePackageMappingConfiguration : IEntityTypeConfiguration<ServicePackageMapping>
    {
        public void Configure(EntityTypeBuilder<ServicePackageMapping> builder)
        {
            builder.HasKey(sp => sp.ServicePackageMappingId);

            builder.ToTable("ServicePackageMappings");
            // n-1: ServicePackageMapping - ServicePackage
            builder.HasOne(sp => sp.ServicePackage)
                   .WithMany(sp => sp.ServicePackageMappings)
                   .HasForeignKey(sp => sp.ServicePackageId);
            // n-1: ServicePackageMapping - Service
            builder.HasOne(sp => sp.ProductType)
                   .WithMany(s => s.ServicePackageMappings)
                   .HasForeignKey(sp => sp.ProductTypeId);
        }
    }
}
