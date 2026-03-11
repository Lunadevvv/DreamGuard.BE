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
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            // 1-1 customer - user
            builder.ToTable("Customers");
            builder.HasKey(x => x.CustomerId);
            builder.HasOne(x => x.User)
                   .WithOne(x => x.Customer)
                   .HasForeignKey<Customer>(x => x.CustomerId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
