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
    public class BabyProfileConfiguration : IEntityTypeConfiguration<BabyProfile>
    {
        public void Configure(EntityTypeBuilder<BabyProfile> builder)
        {
            builder.ToTable("BabyProfiles");

            builder.HasKey(x => x.BabyId);

            builder.Property(x => x.CustomerId)
                   .IsRequired();
        }
    }
}
