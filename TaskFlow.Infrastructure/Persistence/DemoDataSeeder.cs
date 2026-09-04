using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence;

public static class DemoDataSeeder
{
    public const string DemoPassword = "Demo@12345";

    public static async Task ResetAndSeedAsync(
        AppDbContext context,
        IUserPasswordHasher passwordHasher,
        CancellationToken cancellationToken = default)
    {
        if (!context.Database.IsRelational())
            throw new InvalidOperationException("Demo reset is only supported for a relational development database.");

        await context.Database.EnsureDeletedAsync(cancellationToken);
        await context.Database.MigrateAsync(cancellationToken);
        await DatabaseSeeder.SeedReferenceDataAsync(context, cancellationToken);

        var client = new Client
        {
            Name = "TaskFlow Product Lab",
            ContactEmail = "team@taskflow.demo",
            IsActive = true
        };
        context.Clients.Add(client);
        await context.SaveSystemChangesAsync(cancellationToken);
        await DatabaseSeeder.SeedReferenceDataAsync(context, cancellationToken);

        // AddClientTenancy creates a legacy Guid.Empty tenant for old databases. A fresh demo
        // workspace must contain exactly one real client, so remove that migration placeholder.
        var legacyStatuses = await context.Statuses.IgnoreQueryFilters()
            .Where(status => status.ClientId == Guid.Empty).ToListAsync(cancellationToken);
        var legacyDependencies = await context.DependencyTypes.IgnoreQueryFilters()
            .Where(type => type.ClientId == Guid.Empty).ToListAsync(cancellationToken);
        var legacyClient = await context.Clients.FirstOrDefaultAsync(item => item.Id == Guid.Empty, cancellationToken);
        context.Statuses.RemoveRange(legacyStatuses);
        context.DependencyTypes.RemoveRange(legacyDependencies);
        if (legacyClient is not null) context.Clients.Remove(legacyClient);
        await context.SaveSystemChangesAsync(cancellationToken);

        var roles = await context.Roles.ToDictionaryAsync(role => role.RoleCode, cancellationToken);
        var statuses = await context.Statuses.IgnoreQueryFilters()
            .Where(status => status.ClientId == client.Id)
            .ToDictionaryAsync(status => status.Name!, cancellationToken);
        var dependencyTypes = await context.DependencyTypes.IgnoreQueryFilters()
            .Where(type => type.ClientId == client.Id)
            .ToDictionaryAsync(type => type.Name!, cancellationToken);

        var password = passwordHasher.HashPassword(DemoPassword);
        var hammadi = User("Hammadi Al Hammadi", "hammadi@taskflow.demo", roles["ADMIN"], true, password, client.Id);
        var abdullah = User("Abdullah Hijazi", "abdullah@taskflow.demo", roles["MEMBER"], false, password, client.Id);
        var hamza = User("Hamza Al Saado", "hamza@taskflow.demo", roles["MEMBER"], true, password, client.Id);
        var abdulHalim = User("Abdul Halim Al Saleh", "abdulhalim@taskflow.demo", roles["MANAGER"], false, password, client.Id);
        context.Users.AddRange(hammadi, abdullah, hamza, abdulHalim);

        var today = DateTime.UtcNow.Date;
        var platform = Initiative("TaskFlow 2.0 Product Platform", "Build a fast, secure, scalable, and integrated work management platform as the team's core product.", today.AddDays(-50), today.AddDays(55), "#4F46E5", "ti ti-layout-dashboard", statuses["In Progress"], hammadi, client.Id);
        var backend = Initiative("Backend Reliability & API", "Improve API reliability, standardize contracts, optimize performance, expand testing, and strengthen operational monitoring.", today.AddDays(-42), today.AddDays(38), "#2563EB", "ti ti-server", statuses["In Progress"], abdullah, client.Id);
        var aiUx = Initiative("ORQIST AI & Frontend Experience", "Deliver an intelligent assistant and a polished user experience through clear, fast, and responsive interfaces.", today.AddDays(-35), today.AddDays(48), "#7C3AED", "ti ti-sparkles", statuses["In Progress"], hamza, client.Id);
        var discovery = Initiative("Product Analysis & Documentation", "Establish requirements, system documentation, process maps, acceptance criteria, and delivery standards.", today.AddDays(-55), today.AddDays(25), "#0891B2", "ti ti-file-analytics", statuses["At Risk"], abdulHalim, client.Id);
        var release = Initiative("Production Readiness & Launch", "Prepare the pilot release through acceptance testing, security validation, deployment planning, and support readiness.", today.AddDays(5), today.AddDays(70), "#059669", "ti ti-rocket", statuses["Planned"], hammadi, client.Id);
        context.Initiatives.AddRange(platform, backend, aiUx, discovery, release);

        var tasks = new List<TaskItem>
        {
            Task("Standardize Project Architecture and Coding Standards", "Organize modules, naming conventions, automated reviews, and responsibility boundaries between frontend and backend.", -50, -35, 100, platform, hammadi, statuses["Completed"], "#4F46E5", "ti ti-folders", client.Id),
            Task("Design the Core User Journey", "Review initiative, task, and dashboard workflows and eliminate unnecessary user steps.", -28, -8, 85, platform, hammadi, statuses["In Progress"], "#6366F1", "ti ti-route", client.Id),
            Task("Improve Overall Application Performance", "Analyze bundle sizes and queries to reduce initial load time and unnecessary rendering.", -5, 25, 35, platform, hammadi, statuses["In Progress"], "#4338CA", "ti ti-gauge", client.Id),
            Task("Conduct a Comprehensive Security Review", "Audit authentication, secrets, permissions, and tenant data isolation before launch.", 18, 45, 0, platform, hammadi, statuses["Planned"], "#DC2626", "ti ti-shield-check", client.Id),

            Task("Standardize PagedResult and API Contracts", "Unify list, search, filter, and pagination responses across all endpoints.", -42, -24, 100, backend, abdullah, statuses["Completed"], "#2563EB", "ti ti-api", client.Id),
            Task("Expand Integration Test Coverage", "Cover authentication, tenant isolation, 400 and 404 responses, and prevent unexpected 500 errors.", -20, 5, 72, backend, abdullah, statuses["In Progress"], "#1D4ED8", "ti ti-test-pipe", client.Id),
            Task("Optimize Queries and Database Indexes", "Analyze high-traffic queries, add indexes, and eliminate unnecessary data loading.", 3, 22, 20, backend, abdullah, statuses["In Progress"], "#0284C7", "ti ti-database-cog", client.Id),
            Task("Add Health Checks and Monitoring", "Expose database and AI health checks and record production health indicators.", 20, 38, 0, backend, abdullah, statuses["New"], "#0369A1", "ti ti-heart-rate-monitor", client.Id),

            Task("Develop the Search and Filter Experience", "Deliver backend search with URL state, skeleton loading, shareable views, and navigation restoration.", -35, -12, 100, aiUx, hamza, statuses["Completed"], "#7C3AED", "ti ti-filter-search", client.Id),
            Task("Improve Kanban and Gantt Views", "Improve information density, timeline range, interactions, and small-screen responsiveness.", -10, 12, 68, aiUx, hamza, statuses["In Progress"], "#8B5CF6", "ti ti-chart-gantt", client.Id),
            Task("Build the AI Suggestion Save Experience", "Review and select suggestions, then save them in one batch with clear success feedback.", -18, 2, 90, aiUx, hamza, statuses["In Progress"], "#A855F7", "ti ti-wand", client.Id),
            Task("Build Empty, Error, and Skeleton States", "Standardize loading, empty, and error states across all primary pages.", 5, 25, 30, aiUx, hamza, statuses["In Progress"], "#9333EA", "ti ti-loader", client.Id),
            Task("Evaluate ORQIST Output Quality", "Create evaluation scenarios for initiative generation, task generation, and risk analysis quality.", 22, 48, 0, aiUx, hamza, statuses["New"], "#C026D3", "ti ti-brain", client.Id),

            Task("Define Product Scope and Requirements", "Document scope, roles, constraints, and functional and non-functional requirements.", -55, -38, 100, discovery, abdulHalim, statuses["Completed"], "#0891B2", "ti ti-file-description", client.Id),
            Task("Create Process Maps and Use Cases", "Document primary workflows, exceptions, and responsibilities for every role.", -37, -15, 100, discovery, abdulHalim, statuses["Completed"], "#0E7490", "ti ti-sitemap", client.Id),
            Task("Document the Data Dictionary and Integration Contracts", "Document entities, fields, validation rules, and integration contracts between layers.", -14, 8, 60, discovery, abdulHalim, statuses["At Risk"], "#0F766E", "ti ti-book-2", client.Id),
            Task("Prepare User and Operations Guides", "Create a concise user guide plus setup, operations, and support documentation for the technical team.", 5, 25, 25, discovery, abdulHalim, statuses["In Progress"], "#14B8A6", "ti ti-notebook", client.Id),

            Task("Create the User Acceptance Test Plan", "Build UAT scenarios covering roles, critical journeys, and measurable success criteria.", 5, 25, 10, release, abdulHalim, statuses["Planned"], "#059669", "ti ti-checklist", client.Id),
            Task("Resolve Pilot Release Feedback", "Prioritize feedback by impact and close release blockers before deployment.", 26, 48, 0, release, hammadi, statuses["New"], "#16A34A", "ti ti-bug", client.Id),
            Task("Configure CI/CD and Production", "Build a secure pipeline with migrations, health checks, and a rollback plan.", 35, 58, 0, release, abdullah, statuses["Planned"], "#15803D", "ti ti-cloud-upload", client.Id),
            Task("Launch the Release and Monitor Stability", "Execute the launch checklist, monitor logs and performance, and support users.", 58, 70, 0, release, hammadi, statuses["Planned"], "#047857", "ti ti-rocket", client.Id)
        };
        context.Tasks.AddRange(tasks);

        var finishToStart = dependencyTypes["Finish to Start"];
        context.TaskDependencies.AddRange(
            Dependency(tasks[0], tasks[1], finishToStart, client.Id),
            Dependency(tasks[4], tasks[5], finishToStart, client.Id),
            Dependency(tasks[8], tasks[9], finishToStart, client.Id),
            Dependency(tasks[13], tasks[14], finishToStart, client.Id),
            Dependency(tasks[15], tasks[16], finishToStart, client.Id),
            Dependency(tasks[17], tasks[18], finishToStart, client.Id),
            Dependency(tasks[18], tasks[19], finishToStart, client.Id),
            Dependency(tasks[19], tasks[20], finishToStart, client.Id));

        context.Comments.AddRange(
            Comment(tasks[5], hammadi, "Please add a two-tenant data isolation test before closing this task.", client.Id),
            Comment(tasks[5], abdullah, "The scenario is now covered. I will complete permission and pagination cases today.", client.Id),
            Comment(tasks[9], abdulHalim, "The Gantt view should reflect the actual task date range in the UAT scenario.", client.Id),
            Comment(tasks[10], hamza, "Saving is now handled as one transactional batch with clear snackbar feedback.", client.Id),
            Comment(tasks[15], abdullah, "Please finalize field names before the integration contracts are published.", client.Id));

        await context.SaveSystemChangesAsync(cancellationToken);
    }

    private static User User(string name, string email, Role role, bool ai, string password, Guid clientId) => new()
    {
        Name = name, Email = email, PhoneNumber = "+963 900 000 000", Password = password,
        RoleId = role.RoleId, CanAccessAi = ai, ClientId = clientId
    };

    private static Initiative Initiative(string name, string description, DateTime start, DateTime end, string color, string icon, Status status, User owner, Guid clientId) => new()
    {
        Name = name, Description = description, StartDate = start, EndDate = end, Progress = 0,
        IsAISuggested = false, IsActive = true, Color = color, Icon = icon,
        StatusId = status.Id, AssignedToId = owner.Id, ClientId = clientId
    };

    private static TaskItem Task(string name, string description, int startOffset, int endOffset, decimal progress, Initiative initiative, User owner, Status status, string color, string icon, Guid clientId) => new()
    {
        Name = name, Description = description, StartDate = DateTime.UtcNow.Date.AddDays(startOffset),
        EndDate = DateTime.UtcNow.Date.AddDays(endOffset), Progress = progress, IsAISuggested = false,
        IsActive = true, Color = color, Icon = icon, InitiativeId = initiative.Id,
        StatusId = status.Id, AssignedToId = owner.Id, ClientId = clientId
    };

    private static TaskDependency Dependency(TaskItem predecessor, TaskItem successor, DependencyType type, Guid clientId) => new()
    {
        PredecessorId = predecessor.Id, SuccessorId = successor.Id, DependencyTypeId = type.Id, ClientId = clientId
    };

    private static Comment Comment(TaskItem task, User user, string content, Guid clientId) => new()
    {
        TaskId = task.Id, UserId = user.Id, Content = content, ClientId = clientId
    };
}
