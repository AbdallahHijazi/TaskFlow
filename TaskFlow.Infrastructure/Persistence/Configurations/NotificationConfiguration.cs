using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notification");
        builder.Property(item => item.Type).HasMaxLength(64).IsRequired();
        builder.Property(item => item.Title).HasMaxLength(200).IsRequired();
        builder.Property(item => item.Message).HasMaxLength(2000).IsRequired();
        builder.HasIndex(item => new { item.RecipientUserId, item.IsRead, item.CreatedAt });
        builder.HasOne(item => item.RecipientUser).WithMany().HasForeignKey(item => item.RecipientUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(item => item.Task).WithMany().HasForeignKey(item => item.TaskId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(item => item.Initiative).WithMany().HasForeignKey(item => item.InitiativeId).OnDelete(DeleteBehavior.SetNull);
    }
}
