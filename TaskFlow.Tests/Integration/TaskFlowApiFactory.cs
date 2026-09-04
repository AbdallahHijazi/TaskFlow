using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Tests.Integration;

public class TaskFlowApiFactory : WebApplicationFactory<Program>
{
    public const string TestJwtKey = "TEST_ONLY_SIGNING_KEY_WITH_AT_LEAST_32_BYTES_2026";
    private readonly string _databaseName = $"TaskFlowApiTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("Jwt:SecretKey", TestJwtKey);
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = TestJwtKey
            });
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            var dbContextConfigDescriptors = services
                .Where(d => d.ServiceType.IsGenericType &&
                            d.ServiceType.GetGenericTypeDefinition().Name == "IDbContextOptionsConfiguration`1" &&
                            d.ServiceType.GenericTypeArguments[0] == typeof(AppDbContext))
                .ToList();

            foreach (var descriptor in dbContextConfigDescriptors)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}
