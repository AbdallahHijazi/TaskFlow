using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Client");
        builder.Property(c => c.Name).HasMaxLength(160).IsRequired();
        builder.Property(c => c.ContactEmail).HasMaxLength(256);
        builder.HasIndex(c => c.Name).IsUnique();
    }
}
