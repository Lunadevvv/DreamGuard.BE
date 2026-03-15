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
    public class ServiceConfiguration : IEntityTypeConfiguration<Service>
    {
        public void Configure(EntityTypeBuilder<Service> builder)
        {
            builder.ToTable("Services");

            builder.HasKey(e => e.ServiceId);
            // 1 - N ServiceAssets
            builder.HasMany(e => e.ServiceAssets)
                   .WithOne(e => e.Service)
                   .HasForeignKey(e => e.ServiceId);

        }
    }
}
