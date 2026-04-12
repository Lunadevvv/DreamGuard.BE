using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.DAL.Configurations
{
    public class ShippingTaskConfiguration : IEntityTypeConfiguration<ShippingTask>
    {
        public void Configure(EntityTypeBuilder<ShippingTask> builder)
        {
            builder.HasKey(x => x.ShippingTaskId);
            builder.ToTable("ShippingTasks");
            builder.Property(x => x.Status)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(x => x.StaffNote)
                   .HasMaxLength(1000);

            builder.HasOne(x => x.Staff)
                   .WithMany(s => s.ShippingTasks)
                   .HasForeignKey(x => x.StaffId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Order)
                   .WithMany(o => o.ShippingTasks)
                   .HasForeignKey(x => x.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);
            //1 tradeInOrder - n shippingTask
            builder.HasOne(x => x.TradeInOrder)
                   .WithMany(t => t.ShippingTasks)
                   .HasForeignKey(x => x.TradeInOrderId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
