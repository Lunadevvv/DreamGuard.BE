using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace DreamGuard.BE.DAL.Configurations
{
    public class ServiceTaskConfiguration : IEntityTypeConfiguration<ServiceTask>
    {
        public void Configure(EntityTypeBuilder<ServiceTask> builder)
        {
            builder.HasKey(x => x.ServiceTaskId);
            builder.ToTable("ServiceTasks");

            // 1-n Task - ServiceEvidence
            builder.HasMany(t => t.ServiceEvidences)
                   .WithOne(e => e.ServiceTask)
                   .HasForeignKey(e => e.ServiceTaskId);
            // 1-n ServiceTask - Staff
            builder.HasOne(t => t.Staff)
                   .WithMany(t => t.ServiceTasks)
                   .HasForeignKey(t => t.StaffId);
            // 1-1 ServiceTask - ServiceOrder
            builder.HasOne(t => t.ServiceOrder)
                   .WithMany(o => o.ServiceTasks)
                   .HasForeignKey(t => t.SoId);
        }
    }
}
