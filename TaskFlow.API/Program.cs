using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using TaskFlow.API;
using TaskFlow.API.Infrastructure;
using TaskFlow.API.Services;
using TaskFlow.Application.Common.Behaviors;
using TaskFlow.Application.Features.Statuses.Commands;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Infrastructure;
using TaskFlow.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddScoped<TaskFlow.Application.Common.Interfaces.INotificationRealtimePublisher, SignalRNotificationPublisher>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiPresentation(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("password-reset", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
});

builder.Services.AddValidatorsFromAssembly(typeof(CreateStatusCommand).Assembly);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateStatusCommand).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (dbContext.Database.IsRelational())
        await dbContext.Database.MigrateAsync();

    await DatabaseSeeder.SeedReferenceDataAsync(dbContext);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// يفضل تعطيله على Render
// app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseStaticFiles();

app.UseRouting();
app.UseRateLimiter();

app.UseCors("Default");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<TaskFlow.API.Hubs.NotificationsHub>("/hubs/notifications");
app.MapGet("/", () => "TaskFlow API is running");
app.Run();

public partial class Program { }
