using FluentValidation;
using TaskFlow.API;
using TaskFlow.API.Infrastructure;
using TaskFlow.Application.Common.Behaviors;
using TaskFlow.Application.Features.Statuses.Commands;
using TaskFlow.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiPresentation(builder.Configuration);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddValidatorsFromAssembly(typeof(CreateStatusCommand).Assembly);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateStatusCommand).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseStaticFiles();

app.UseCors("Default");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
