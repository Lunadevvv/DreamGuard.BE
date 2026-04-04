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
    public class TradeInOrderConfiguration : IEntityTypeConfiguration<TradeInOrder>
    {
        public void Configure(EntityTypeBuilder<TradeInOrder> builder)
        {
            builder.ToTable("TradeInOrders");
            builder.HasKey(tio => tio.TradeInOrderId);
            
            //1-n TradeInImage
            builder.HasMany(tio => tio.TradeInImages)
                   .WithOne(ti => ti.TradeInOrder)
                   .HasForeignKey(ti => ti.TradeInOrderId)
                   .OnDelete(DeleteBehavior.Cascade);
            //1-n Payment
            builder.HasMany(tio => tio.Payments)
                   .WithOne(p => p.TradeInOrder)
                   .HasForeignKey(p => p.TradeInOrderId)
                   .OnDelete(DeleteBehavior.Restrict);
            //1-1 OrderItem
            builder.HasOne(tio => tio.OrderItem)
                   .WithOne(oi => oi.TradeInOrder)
                   .HasForeignKey<TradeInOrder>(tio => tio.POrderItemId)
                   .OnDelete(DeleteBehavior.Restrict);
            // n-1 ProductVariant
            builder.HasOne(tio => tio.ProductVariant)
                   .WithMany(pv => pv.TradeInOrders)
                   .HasForeignKey(tio => tio.ProductVariantId)
                   .OnDelete(DeleteBehavior.Restrict);
            // n-1 Customer
            builder.HasOne(tio => tio.Customer)
                   .WithMany(c => c.TradeInOrders)
                   .HasForeignKey(tio => tio.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
