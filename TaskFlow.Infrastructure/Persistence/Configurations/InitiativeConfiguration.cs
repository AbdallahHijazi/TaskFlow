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
    public class InitiativeConfiguration : IEntityTypeConfiguration<Initiative>
    {
        public void Configure(EntityTypeBuilder<Initiative> builder)
        {
            builder.ToTable("Initiative");

            builder.Property(e => e.Progress).HasColumnType("decimal(5,2)");

            builder.HasOne(e => e.Status)
                .WithMany(s => s.Initiatives)
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.AssignedTo)
                .WithMany(u => u.AssignedInitiatives)
                .HasForeignKey(e => e.AssignedToId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Image)
                .WithMany(i => i.Initiatives)
                .HasForeignKey(e => e.ImageId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
