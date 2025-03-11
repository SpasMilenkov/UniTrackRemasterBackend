using System.Text.Json.Serialization;
using Infrastructure;
using Prometheus;
using UniTrackRemaster.Data.Seeding;

var builder = WebApplication.CreateBuilder(args);

AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
{
    Console.WriteLine($"Unhandled exception: {eventArgs.ExceptionObject}");
};

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });;
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddJwtToken(builder.Configuration);
builder.Services.AddServices(builder.Configuration);
builder.Services.AddSwagger();
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
    app.MapControllers();
    app.UseMetricServer();
    app.UseHttpMetrics(); 
    CookieOptionManager.Initialize(builder.Configuration);
    await DataSeeder.SeedData(app.Services);
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    
    app.Lifetime.ApplicationStopping.Register(() =>
    {
        Console.WriteLine("Application is stopping");
    });

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Application startup failed: {ex}");
    throw;
}
