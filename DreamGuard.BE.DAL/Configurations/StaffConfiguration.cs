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
    public class StaffConfiguration : IEntityTypeConfiguration<Staff>
    {
        public void Configure(EntityTypeBuilder<Staff> builder)
        {
            //1-1 staff - user
            builder.ToTable("Staffs");
            builder.HasKey(x => x.StaffId);
            builder.HasOne(x => x.User)
                   .WithOne(x => x.Staff)
                   .HasForeignKey<Staff>(x => x.StaffId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
