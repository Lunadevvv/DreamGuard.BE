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
            // 1-n: ServiceOrder - Payment
            builder.HasMany(so => so.Payments)
                   .WithOne(p => p.ServiceOrder)
                   .HasForeignKey(p => p.SoId);
            // 1-n : ServiceOrder - ServiceAsset
            builder.HasMany(so => so.ServiceAssets)
                   .WithOne(sa => sa.ServiceOrder)
                   .HasForeignKey(sa => sa.ServiceOrderId);
            // 1-1 : ServiceOrder - UserVoucher
            builder.HasOne(so => so.UserVoucher)
                   .WithOne(uv => uv.ServiceOrder)
                   .HasForeignKey<ServiceOrder>(so => so.UserVoucherId);
        }
    }
}
