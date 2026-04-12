using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations
{
    public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
    {
        public void Configure(EntityTypeBuilder<TaskItem> builder)
        {
            builder.ToTable("Task");

            builder.Property(e => e.Progress).HasColumnType("decimal(5,2)");

            builder.HasOne(e => e.Initiative)
                    .WithMany(i => i.Tasks)
                    .HasForeignKey(e => e.InitiativeId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Image)
                    .WithMany(i => i.Tasks)
                    .HasForeignKey(e => e.ImageId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Status)
                    .WithMany(s => s.Tasks)
                    .HasForeignKey(e => e.StatusId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.AssignedTo)
                    .WithMany(u => u.AssignedTasks)
                    .HasForeignKey(e => e.AssignedToId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Parent)
                    .WithMany(e => e.Children)
                    .HasForeignKey(e => e.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
