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
    public class RatingConfiguration : IEntityTypeConfiguration<Rating>
    {
        public void Configure(EntityTypeBuilder<Rating> builder)
        {
            builder.HasKey(r => r.RatingId);
            builder.ToTable("Ratings");
            // n-1 rating - staff
            builder.HasOne(s => s.Staff)
                .WithMany(r => r.Ratings)
                .HasForeignKey(r => r.StaffId)
                .OnDelete(DeleteBehavior.Cascade);
            // 1-1 rating - service order
            builder.HasOne(so => so.ServiceOrder)
                .WithOne(r => r.Rating)
                .HasForeignKey<Rating>(r => r.ServiceOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
