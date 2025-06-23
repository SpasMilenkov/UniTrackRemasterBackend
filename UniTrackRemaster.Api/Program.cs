using System.Text.Json.Serialization;
using AspNetCoreRateLimit;
using Infrastructure;
using Microsoft.AspNetCore.Http.Connections;
using Prometheus;
using UniTrackRemaster.Data.Seeding;
using UniTrackRemaster.EventHandlers;
using UniTrackRemaster.Hubs;

var builder = WebApplication.CreateBuilder(args);

AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
{
    Console.WriteLine($"Unhandled exception: {eventArgs.ExceptionObject}");
};

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddJwtToken(builder.Configuration);
builder.Services.AddServices(builder.Configuration);
builder.Services.AddOpenTelemetry(builder.Configuration);
builder.Services.AddMetricsServices();
builder.Services.ConfigureSignalR(builder.Configuration, builder.Environment);
builder.Services.AddSwagger();
builder.Services.AddHostedService<ChatSignalREventHandler>();
// builder.WebHost.UseKestrel()
//                .UseUrls("http://*:5086");
builder.Services.AddHealthChecks();

var app = builder.Build();

try
{
    app.ApplyMigrations();
    app.UseRouting();
    app.UseCors("AllowOrigin");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseWebSockets();
    app.MapHub<ChatHub>("/hubs/chat", options =>
    {
        options.Transports = HttpTransportType.WebSockets | HttpTransportType.ServerSentEvents | HttpTransportType.LongPolling;
    });
    app.MapControllers();
    app.UseMetricServer();
    app.UseHttpMetrics();
    app.UseIpRateLimiting();
    CookieOptionManager.Initialize(builder.Configuration);

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.Lifetime.ApplicationStopping.Register(() =>
    {
        Console.WriteLine("Application is stopping");
    });
    app.UseMetricsMiddleware();
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Application startup failed: {ex}");
    throw;
}
