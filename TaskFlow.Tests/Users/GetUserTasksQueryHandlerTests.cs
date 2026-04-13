using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs.User;
using TaskFlow.Application.Features.Users.Commands;
using TaskFlow.Application.Features.Users.Handlers;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Persistence.Repositories;

namespace TaskFlow.Tests.Users;

public class GetUserTasksQueryHandlerTests
{
    [Fact]
    public async Task Handle_UserExistsWithTasks_ReturnsTasks()
    {
        await using var context = CreateContext();
        var userId = SeedUserWithTasks(context, taskCount: 2);
        var handler = CreateHandler(context);

        var result = await handler.Handle(
            new GetUserTasksQuery(userId, new UserTasksQueryParametersDto()),
            CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task Handle_UserExistsWithNoTasks_ReturnsEmptyList()
    {
        await using var context = CreateContext();
        var userId = SeedUserWithTasks(context, taskCount: 0);
        var handler = CreateHandler(context);

        var result = await handler.Handle(
            new GetUserTasksQuery(userId, new UserTasksQueryParametersDto()),
            CancellationToken.None);

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFound()
    {
        await using var context = CreateContext();
        var handler = CreateHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new GetUserTasksQuery(Guid.NewGuid(), new UserTasksQueryParametersDto()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Pagination_Works()
    {
        await using var context = CreateContext();
        var userId = SeedUserWithTasks(context, taskCount: 5);
        var handler = CreateHandler(context);

        var result = await handler.Handle(
            new GetUserTasksQuery(userId, new UserTasksQueryParametersDto { PageNumber = 2, PageSize = 2 }),
            CancellationToken.None);

        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task Handle_FilteringSortingAndSearch_Works()
    {
        await using var context = CreateContext();
        var userId = Guid.NewGuid();
        var statusA = Guid.NewGuid();
        var statusB = Guid.NewGuid();
        var initiativeA = Guid.NewGuid();

        context.Users.Add(new User { Id = userId, Name = "User A", Email = "a@test.com", Password = "x" });
        context.Tasks.AddRange(
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = "Design API",
                Description = "Core work",
                AssignedToId = userId,
                StatusId = statusA,
                InitiativeId = initiativeA,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 1, 10),
                CreatedAt = new DateTime(2026, 1, 1),
                Progress = 0.9m
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = "Write docs",
                Description = "Documentation",
                AssignedToId = userId,
                StatusId = statusB,
                InitiativeId = initiativeA,
                StartDate = new DateTime(2026, 2, 1),
                EndDate = new DateTime(2026, 2, 20),
                CreatedAt = new DateTime(2026, 2, 1),
                Progress = 0.2m
            });
        await context.SaveChangesAsync();

        var handler = CreateHandler(context);
        var result = await handler.Handle(
            new GetUserTasksQuery(userId, new UserTasksQueryParametersDto
            {
                Status = statusA,
                InitiativeId = initiativeA,
                FromDate = new DateTime(2026, 1, 1),
                ToDate = new DateTime(2026, 1, 31),
                Search = "design",
                SortBy = "dueDate",
                SortDirection = "asc"
            }),
            CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("Design API", result.Items[0].Name);
    }

    private static GetUserTasksQueryHandler CreateHandler(AppDbContext context)
    {
        return new GetUserTasksQueryHandler(
            new GenericRepository<User>(context),
            new GenericRepository<TaskItem>(context));
    }

    private static Guid SeedUserWithTasks(AppDbContext context, int taskCount)
    {
        var userId = Guid.NewGuid();
        context.Users.Add(new User
        {
            Id = userId,
            Name = "User A",
            Email = "user@test.com",
            Password = "x"
        });

        for (var i = 0; i < taskCount; i++)
        {
            context.Tasks.Add(new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = $"Task {i + 1}",
                AssignedToId = userId,
                CreatedAt = DateTime.UtcNow.AddDays(-i),
                EndDate = DateTime.UtcNow.AddDays(i)
            });
        }

        context.SaveChanges();
        return userId;
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
