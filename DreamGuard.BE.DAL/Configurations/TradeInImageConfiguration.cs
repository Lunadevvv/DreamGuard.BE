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
    public class TradeInImageConfiguration : IEntityTypeConfiguration<TradeInImage>
    {
        public void Configure(EntityTypeBuilder<TradeInImage> builder)
        {
            builder.ToTable("TradeInImages");
            builder.HasKey(ti => ti.TradeInImageId);
        }
    }
}
