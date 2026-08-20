using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedReferenceDataAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        var roleCodes = await context.Roles.Select(role => role.RoleCode).ToListAsync(cancellationToken);
        AddRoleIfMissing(context, roleCodes, "ADMIN", "Admin");
        AddRoleIfMissing(context, roleCodes, "MANAGER", "Manager");
        AddRoleIfMissing(context, roleCodes, "MEMBER", "Member");

        var clientIds = await context.Clients.Select(client => client.Id).ToListAsync(cancellationToken);
        foreach (var clientId in clientIds)
        {
            var statusNames = await context.Statuses.IgnoreQueryFilters().Where(status => status.ClientId == clientId).Select(status => status.Name).ToListAsync(cancellationToken);
            AddStatusIfMissing(context, statusNames, clientId, "Planned", "Work that has not started yet.", "#64748B");
            AddStatusIfMissing(context, statusNames, clientId, "In Progress", "Work currently in progress.", "#2563EB");
            AddStatusIfMissing(context, statusNames, clientId, "At Risk", "Work requiring attention.", "#DC2626");
            AddStatusIfMissing(context, statusNames, clientId, "Completed", "Work completed successfully.", "#059669");

            var dependencyNames = await context.DependencyTypes.IgnoreQueryFilters().Where(type => type.ClientId == clientId).Select(type => type.Name).ToListAsync(cancellationToken);
            AddDependencyIfMissing(context, dependencyNames, clientId, "Finish to Start", "The predecessor must finish before the successor can start.");
            AddDependencyIfMissing(context, dependencyNames, clientId, "Start to Start", "Both tasks start together.");
            AddDependencyIfMissing(context, dependencyNames, clientId, "Finish to Finish", "Both tasks finish together.");
            AddDependencyIfMissing(context, dependencyNames, clientId, "Start to Finish", "The successor cannot finish until the predecessor starts.");
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static void AddRoleIfMissing(AppDbContext context, IEnumerable<string> existingCodes, string code, string name)
    {
        if (!existingCodes.Any(value => string.Equals(value, code, StringComparison.OrdinalIgnoreCase)))
            context.Roles.Add(new Role { RoleCode = code, RoleName = name });
    }

    private static void AddStatusIfMissing(AppDbContext context, IEnumerable<string?> existingNames, Guid clientId, string name, string description, string color)
    {
        if (!existingNames.Any(value => string.Equals(value, name, StringComparison.OrdinalIgnoreCase)))
            context.Statuses.Add(new Status { ClientId = clientId, Name = name, Description = description, Color = color });
    }

    private static void AddDependencyIfMissing(AppDbContext context, IEnumerable<string?> existingNames, Guid clientId, string name, string description)
    {
        if (!existingNames.Any(value => string.Equals(value, name, StringComparison.OrdinalIgnoreCase)))
            context.DependencyTypes.Add(new DependencyType { ClientId = clientId, Name = name, Description = description });
    }
}
