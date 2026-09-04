using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetToken");
        builder.Property(item => item.TokenHash).HasMaxLength(64).IsRequired();
        builder.HasIndex(item => item.TokenHash).IsUnique();
        builder.HasIndex(item => new { item.UserId, item.ExpiresAtUtc });
        builder.HasOne(item => item.User).WithMany(user => user.PasswordResetTokens)
            .HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
