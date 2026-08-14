using DevForge.Application.Common.Interfaces;
using DevForge.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DevForge.Application;

/// <summary>
/// Dependency injection registration for the Application layer.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IJsonFormatterService, JsonFormatterService>();
        services.AddSingleton<IJwtInspectorService, JwtInspectorService>();
        services.AddSingleton<ISqlFormatterService, SqlFormatterService>();
        services.AddScoped<IApiPlaygroundService, ApiPlaygroundService>();
        
        return services;
    }
}
