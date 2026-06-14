using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Services;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Infrastructure.AI;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Persistence.Repositories;
using TaskFlow.Infrastructure.Security;
using TaskFlow.Infrastructure.Storage;


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
            services.Configure<OllamaOptions>(
                     configuration.GetSection("Ollama"));

            services.AddHttpClient<IAiChatService, OllamaChatService>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<OllamaOptions>>().Value;

                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });
            return services;
        }
    }
}
