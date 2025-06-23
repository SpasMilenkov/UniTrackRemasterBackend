using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Infrastructure;

public static class OpenTelemetryExtensions
{
    public static void AddOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceName = "UniTrackRemaster";
        var serviceVersion = "1.0.0";

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
            .AddTelemetrySdk()
            .AddEnvironmentVariableDetector();

        services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resourceBuilder)
                    .AddRuntimeInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter("UniTrack.Metrics");
            })
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddSource("UniTrack.Tracing")
                    .AddConsoleExporter()
                    .AddOtlpExporter(opts => // For sending to a collector (like Jaeger)
                    {
                        opts.Endpoint = new Uri(configuration.GetValue<string>("OpenTelemetry:OtlpEndpoint") ?? "http://localhost:4317");
                    });
            });
    }
}