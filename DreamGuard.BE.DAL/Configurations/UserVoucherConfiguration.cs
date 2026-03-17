using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamGuard.BE.DAL.Configurations
{
    public class UserVoucherConfiguration : IEntityTypeConfiguration<UserVoucher>
    {
        public void Configure(EntityTypeBuilder<UserVoucher> builder)
        {
            builder.ToTable("UserVouchers");
            builder.HasKey(uv => uv.UserVoucherId);
            builder.HasOne(uv => uv.Voucher)
                     .WithMany(v => v.UserVouchers)
                     .HasForeignKey(uv => uv.VoucherId)
                     .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
