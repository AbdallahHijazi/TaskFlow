using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Features.Users.Commands;
using TaskFlow.Application.Features.Users.Handlers;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Persistence.Repositories;

namespace TaskFlow.Tests.Users;

public class GetUserProfileWithTasksQueryHandlerTests
{
    [Fact]
    public async Task Handle_UserExists_ReturnsProfileAndTasks()
    {
        await using var context = CreateContext();
        var role = new Role { RoleId = Guid.NewGuid(), RoleName = "Developer" };
        var userId = Guid.NewGuid();

        context.Roles.Add(role);
        context.Users.Add(new User
        {
            Id = userId,
            Name = "Ali",
            Email = "ali@test.com",
            Password = "x",
            RoleId = role.RoleId
        });
        context.Tasks.Add(new TaskItem
        {
            Id = Guid.NewGuid(),
            Name = "Task A",
            AssignedToId = userId,
            EndDate = DateTime.UtcNow.AddDays(2),
            Progress = 0.5m
        });
        await context.SaveChangesAsync();

        var handler = new GetUserProfileWithTasksQueryHandler(new GenericRepository<User>(context));
        var result = await handler.Handle(new GetUserProfileWithTasksQuery(userId), CancellationToken.None);

        Assert.Equal(userId, result.Id);
        Assert.Equal("Ali", result.Name);
        Assert.Equal(1, result.TotalTasksCount);
        Assert.Single(result.Tasks);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFound()
    {
        await using var context = CreateContext();
        var handler = new GetUserProfileWithTasksQueryHandler(new GenericRepository<User>(context));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetUserProfileWithTasksQuery(Guid.NewGuid()), CancellationToken.None));
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
