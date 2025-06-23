using Infrastructure.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Services.Metrics;

namespace Infrastructure;

public static class MetricsServiceExtensions
{
    public static IServiceCollection AddMetricsServices(this IServiceCollection services)
    {
        // Register the metrics service as a singleton so it can maintain state
        services.AddSingleton<MetricsService>();
        services.AddSingleton<IMetricsService>(provider => provider.GetRequiredService<MetricsService>());
        services.AddScoped<IEntityStatisticsService, EntityStatisticsService>();
        return services;
    }

    public static IApplicationBuilder UseMetricsMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<MetricsMiddleware>();
        return app;
    }
}