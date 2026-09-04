using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Tests.Integration;

namespace TaskFlow.Tests.Integration.Users;

public class GetUserTasksEndpointIntegrationTests : IClassFixture<TaskFlowApiFactory>
{
    private static readonly Guid ClientId = Guid.NewGuid();
    private readonly TaskFlowApiFactory _factory;

    public GetUserTasksEndpointIntegrationTests(TaskFlowApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetUserTasks_WithValidOwnerToken_ReturnsSuccess()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await ResetDbAsync(db);

        var userId = Guid.NewGuid();
        db.Clients.Add(new Client { Id = ClientId, Name = "Test Client" });
        db.Users.Add(new User { Id = userId, ClientId = ClientId, Name = "Owner", Email = "owner@test.com", Password = "x" });
        db.Tasks.Add(new TaskItem
        {
            Id = Guid.NewGuid(),
            ClientId = ClientId,
            Name = "Task A",
            AssignedToId = userId,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateJwt(userId, "User"));

        var response = await client.GetAsync($"/api/users/{userId}/tasks?pageNumber=1&pageSize=20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetUserTasks_WithInvalidSortBy_ReturnsBadRequest()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await ResetDbAsync(db);

        var userId = Guid.NewGuid();
        db.Clients.Add(new Client { Id = ClientId, Name = "Test Client" });
        db.Users.Add(new User { Id = userId, ClientId = ClientId, Name = "Owner", Email = "owner@test.com", Password = "x" });
        await db.SaveChangesAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateJwt(userId, "User"));

        var response = await client.GetAsync($"/api/users/{userId}/tasks?sortBy=unknown");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("\"errorCode\":\"400\"", body);
        Assert.Contains("validationErrors", body);
    }

    [Fact]
    public async Task GetUserTasks_WithoutToken_ReturnsUnauthorized()
    {
        var userId = Guid.NewGuid();
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/users/{userId}/tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUserTasks_WithAdminTokenAndMissingUser_ReturnsNotFound()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await ResetDbAsync(db);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateJwt(Guid.NewGuid(), "Admin"));

        var response = await client.GetAsync($"/api/users/{Guid.NewGuid()}/tasks");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("\"errorCode\":\"404\"", body);
    }

    private static async Task ResetDbAsync(AppDbContext db)
    {
        db.Tasks.RemoveRange(db.Tasks.IgnoreQueryFilters());
        db.Users.RemoveRange(db.Users);
        db.Clients.RemoveRange(db.Clients);
        await db.SaveChangesAsync();
    }

    private static string CreateJwt(Guid userId, string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim("client_id", ClientId.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TaskFlowApiFactory.TestJwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "TaskFlow.Dev",
            audience: "TaskFlow.Client.Dev",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
