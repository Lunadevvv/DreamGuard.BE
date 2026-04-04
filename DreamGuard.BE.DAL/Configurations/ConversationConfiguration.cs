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
    public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
    {
        public void Configure(EntityTypeBuilder<Conversation> builder)
        {
            builder.ToTable("Conversations");
            builder.HasKey(c => c.ConversationId);
            //1-1 tradeinorder
            builder.HasOne(c => c.TradeInOrder)
                .WithOne(t => t.Conversation)
                .HasForeignKey<Conversation>(c => c.TradeInOrderId)
                .OnDelete(DeleteBehavior.Restrict);
            //1-n customer
            builder.HasOne(c => c.Customer)
                .WithMany(cu => cu.Conversations)
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            //1-n staff
            builder.HasOne(c => c.Staff)
                .WithMany(s => s.Conversations)
                .HasForeignKey(c => c.StaffId)
                .OnDelete(DeleteBehavior.Restrict);
            //1-n chatmessage
            builder.HasMany(c => c.ChatMessages)
                .WithOne(cm => cm.Conversation)
                .HasForeignKey(cm => cm.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
