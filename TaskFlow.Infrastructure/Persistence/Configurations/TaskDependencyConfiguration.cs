using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations
{
    public class TaskDependencyConfiguration : IEntityTypeConfiguration<TaskDependency>
    {
        public void Configure(EntityTypeBuilder<TaskDependency> builder)
        {
            builder.ToTable("TaskDependency");

            builder.HasOne(e => e.DependencyType)
                .WithMany(d => d.TaskDependencies)
                .HasForeignKey(e => e.DependencyTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Predecessor)
                .WithMany(t => t.PredecessorDependencies)
                .HasForeignKey(e => e.PredecessorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Successor)
                .WithMany(t => t.SuccessorDependencies)
                .HasForeignKey(e => e.SuccessorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
