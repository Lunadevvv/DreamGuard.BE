using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

              builder.Property(oi => oi.DamagedItems)
                     .HasColumnType("jsonb")
                     .HasConversion(
                     v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                     v => SafeDeserialize(v)
                     )
                     .Metadata.SetValueComparer(new ValueComparer<List<DamagedItem>>(
                     (c1, c2) => JsonSerializer.Serialize(c1, (JsonSerializerOptions)null) == JsonSerializer.Serialize(c2, (JsonSerializerOptions)null),
                     c => c == null ? 0 : JsonSerializer.Serialize(c, (JsonSerializerOptions)null).GetHashCode(),
                     c => SafeDeserialize(JsonSerializer.Serialize(c, (JsonSerializerOptions)null))
                     ));

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

              private static List<DamagedItem> SafeDeserialize(string? json)
              {
                     if (string.IsNullOrWhiteSpace(json) || json == "null" || json == "\"\"" || json == "{}")
                     {
                            return new List<DamagedItem>();
                     }

                     try
                     {
                            return JsonSerializer.Deserialize<List<DamagedItem>>(json, (JsonSerializerOptions?)null) ?? new List<DamagedItem>();
                     }
                     catch (JsonException)
                     {
                            // Nếu JSON từ DB bị hỏng hoặc không phải List (VD: chuỗi tóm tắt, object...)
                            return new List<DamagedItem>();
                     }
              }
       }
}
