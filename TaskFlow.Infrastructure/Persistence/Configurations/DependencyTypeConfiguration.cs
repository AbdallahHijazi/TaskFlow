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
    public class DependencyTypeConfiguration : IEntityTypeConfiguration<DependencyType>
    {
        public void Configure(EntityTypeBuilder<DependencyType> builder)
        {
            builder.ToTable("DependencyType");
        }
    }
}
