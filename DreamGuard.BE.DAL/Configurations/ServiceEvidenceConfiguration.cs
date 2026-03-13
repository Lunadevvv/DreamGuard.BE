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
    public class ServiceEvidenceConfiguration : IEntityTypeConfiguration<ServiceEvidence>
    {
        public void Configure(EntityTypeBuilder<ServiceEvidence> builder)
        {
            builder.HasKey(x => x.SeId);
            builder.ToTable("ServiceEvidences");
        }
    }
}
