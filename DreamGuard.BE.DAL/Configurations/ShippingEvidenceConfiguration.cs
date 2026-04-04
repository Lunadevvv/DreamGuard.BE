using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.DAL.Configurations
{
    public class ShippingEvidenceConfiguration : IEntityTypeConfiguration<ShippingEvidence>
    {
        public void Configure(EntityTypeBuilder<ShippingEvidence> builder)
        {
            builder.HasKey(x => x.EvidenceId);

            builder.Property(x => x.EvidenceUrl)
                   .IsRequired();

            builder.Property(x => x.EvidenceType)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.HasOne(x => x.ShippingTask)
                   .WithMany(st => st.ShippingEvidences)
                   .HasForeignKey(x => x.ShippingTaskId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
