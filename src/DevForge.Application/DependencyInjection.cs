using Microsoft.Extensions.DependencyInjection;

namespace DevForge.Application;

/// <summary>
/// Dependency injection registration for the Application layer.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}
