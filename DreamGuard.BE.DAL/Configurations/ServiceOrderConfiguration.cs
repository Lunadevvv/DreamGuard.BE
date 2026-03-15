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
    internal class ServiceOrderConfiguration : IEntityTypeConfiguration<ServiceOrder>
    {
        public void Configure(EntityTypeBuilder<ServiceOrder> builder)
        {
            builder.HasKey(so => so.SoId);

            builder.ToTable("ServiceOrders");
            // n-1: ServiceOrder - Customer
            builder.HasOne(so => so.Customer)
                   .WithMany(u => u.ServiceOrders)
                   .HasForeignKey(so => so.CustomerId);
            // n-1: ServiceOrder - ServicePackageMapping
            builder.HasOne(so => so.ServicePackageMapping)
                   .WithMany(so => so.ServiceOrders)
                   .HasForeignKey(so => so.ServicePackageMappingId);
        }
    }
}
