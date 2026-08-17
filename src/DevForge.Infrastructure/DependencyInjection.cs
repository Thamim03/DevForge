using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DevForge.Application.Common.Interfaces;
using DevForge.Infrastructure.Persistence;
using DevForge.Infrastructure.Services;

namespace DevForge.Infrastructure;

/// <summary>
/// Dependency injection registration for the Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (connectionString.Contains("Mode=Memory") || 
                connectionString.Contains("DataSource=:memory:") || 
                connectionString.Contains("Data Source=:memory:") ||
                connectionString.Contains(".db"))
            {
                options.UseSqlite(connectionString);
            }
            else
            {
                options.UseSqlServer(connectionString,
                    builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
            }
        });

        // Register Database health check
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>("Database");

        // Register Authentication & Initialization Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<DbContextInitializer>();

        // Register Challenge Service
        services.AddScoped<IChallengeService, ChallengeService>();

        return services;
    }
}
