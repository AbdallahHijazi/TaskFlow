using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Tests.Persistence;

public sealed class DatabaseSeederTests
{
    [Fact]
    public async Task SaveSystemChanges_WithLegacyDefaultClientEntity_DoesNotRequireAuthenticatedTenant()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"seeder-{Guid.NewGuid()}")
            .Options;

        await using var context = new AppDbContext(options);
        context.Statuses.Add(new Status { ClientId = Guid.Empty, Name = "Planned" });
        await context.SaveSystemChangesAsync();

        var status = await context.Statuses.IgnoreQueryFilters().SingleAsync();
        Assert.Equal(Guid.Empty, status.ClientId);
    }
}
