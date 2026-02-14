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
    public class UserVoucherConfiguration : IEntityTypeConfiguration<UserVoucher>
    {
        public void Configure(EntityTypeBuilder<UserVoucher> builder)
        {
            builder.ToTable("UserVouchers");
            builder.HasKey(uv => uv.UserVoucherId);
            builder.HasOne(uv => uv.User)
                   .WithMany(u => u.UserVouchers)
                   .HasForeignKey(uv => uv.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(uv => uv.Voucher)
                     .WithMany(v => v.UserVouchers)
                     .HasForeignKey(uv => uv.VoucherId)
                     .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
