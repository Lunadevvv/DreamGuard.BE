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
    public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
    {
        public void Configure(EntityTypeBuilder<Voucher> builder)
        {
            builder.HasKey(e => e.VoucherId);
            builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
            builder.HasIndex(e => e.Code).IsUnique();
            builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
            builder.Property(e => e.StartDate).HasColumnType("TIMESTAMPTZ");
            builder.Property(e => e.EndDate).HasColumnType("TIMESTAMPTZ");
            builder.ToTable("Vouchers");
        }
    }
}
