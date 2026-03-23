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
    public class ServiceOrderItemConfiguration : IEntityTypeConfiguration<ServiceOrderItem>
    {
        public void Configure(EntityTypeBuilder<ServiceOrderItem> builder)
        {
            builder.ToTable("ServiceOrderItems");
            builder.HasKey(soi => soi.ServiceOrderItemId);
            // n-1 : ServiceOrderItem - ServiceOrder
            builder.HasOne(soi => soi.ServiceOrder)
                   .WithMany(so => so.ServiceOrderItems)
                   .HasForeignKey(soi => soi.SoId);
            // n-1 : ServiceOrderItem - ServicePackageMapping
            builder.HasOne(soi => soi.ServicePackageMapping)
                   .WithMany(spm => spm.ServiceOrderItems)
                   .HasForeignKey(soi => soi.ServicePackageMappingId);
        }
    }
}
