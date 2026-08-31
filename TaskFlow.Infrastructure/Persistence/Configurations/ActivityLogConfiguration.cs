using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("ActivityLog");
        builder.Property(item => item.Type).HasMaxLength(64).IsRequired();
        builder.Property(item => item.Description).HasMaxLength(2000).IsRequired();
        builder.Property(item => item.OldValue).HasMaxLength(1000);
        builder.Property(item => item.NewValue).HasMaxLength(1000);
        builder.HasIndex(item => new { item.TaskId, item.CreatedAt });
        builder.HasOne(item => item.ActorUser).WithMany().HasForeignKey(item => item.ActorUserId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(item => item.Task).WithMany().HasForeignKey(item => item.TaskId).OnDelete(DeleteBehavior.SetNull);
    }
}
