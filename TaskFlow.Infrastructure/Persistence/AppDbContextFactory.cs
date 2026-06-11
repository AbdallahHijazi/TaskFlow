using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TaskFlow.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var baseDir = Directory.GetCurrentDirectory();

        var apiProject = Path.Combine(baseDir, "TaskFlow.API");

        if (!Directory.Exists(apiProject))
        {
            apiProject = Path.GetFullPath(Path.Combine(baseDir, "..", "TaskFlow.API"));
        }

        var configuration = new ConfigurationBuilder()
                 .SetBasePath(apiProject)
                 .AddJsonFile("appsettings.json", optional: false)
                 .AddJsonFile("appsettings.Development.json", optional: true)
                 .Build();

        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection was not found.");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}