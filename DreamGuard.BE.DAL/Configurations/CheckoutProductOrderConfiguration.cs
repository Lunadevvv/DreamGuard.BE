using DreamGuard.BE.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamGuard.BE.DAL.Configurations
{
    public class CheckoutProductOrderConfiguration : IEntityTypeConfiguration<CheckoutProductOrder>
    {
        public void Configure(EntityTypeBuilder<CheckoutProductOrder> builder)
        {
            builder.ToTable("CheckoutProductOrders");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.CheckoutOrderCode).HasMaxLength(50).IsRequired();

            builder.HasOne(c => c.Customer)
                   .WithMany()
                   .HasForeignKey(c => c.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.UserVoucher)
                   .WithMany()
                   .HasForeignKey(c => c.UserVoucherId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
