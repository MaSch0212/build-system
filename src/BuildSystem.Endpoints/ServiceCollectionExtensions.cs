using FastEndpoints;
using Microsoft.Extensions.DependencyInjection;

namespace BuildSystem.Endpoints;

public static class ServiceCollectionExtensions
{
    public static void AddBuildSystemEndpoints(this IServiceCollection services)
    {
        services.AddFastEndpoints(new[] { typeof(ServiceCollectionExtensions).Assembly });
    }
}
