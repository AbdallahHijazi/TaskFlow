using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.AI.Providers;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Services;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Infrastructure.AI;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Persistence.Repositories;
using TaskFlow.Infrastructure.Security;
using TaskFlow.Infrastructure.Storage;
using TaskFlow.Infrastructure.Notifications;


namespace TaskFlow.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IImageFileStorage, LocalImageFileStorage>();
            services.AddSingleton<IUserPasswordHasher, UserPasswordHasher>();
            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddSingleton<IAuthSettingsProvider, AuthSettingsProvider>();
            services.AddScoped<IImageService, ImageService>();
            services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
            services.AddScoped<IWorkEventService, WorkEventService>();


            services.Configure<OllamaOptions>
                (configuration.GetSection("Ollama"));

            services.AddHttpClient<IAiChatService, OllamaChatService>(
                (serviceProvider, httpClient) =>
                {
                    var options = serviceProvider
                        .GetRequiredService<IOptions<OllamaOptions>>()
                        .Value;

                    httpClient.BaseAddress = new Uri(options.BaseUrl);
                    httpClient.Timeout =
                        TimeSpan.FromSeconds(options.TimeoutSeconds);
                }
            );


            services.AddHttpClient<ILLMProvider, OllamaProvider>(
                (serviceProvider, httpClient) =>
                {
                    var options = serviceProvider
                        .GetRequiredService<IOptions<OllamaOptions>>()
                        .Value;

                    httpClient.BaseAddress = new Uri(options.BaseUrl);
                    httpClient.Timeout =
                        TimeSpan.FromSeconds(options.TimeoutSeconds);
                });

            return services;
        }
    }
}
