using Infrastructure;
using Prometheus;
using UniTrackRemaster.Data.Seeding;

var builder = WebApplication.CreateBuilder(args);

AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
{
    Console.WriteLine($"Unhandled exception: {eventArgs.ExceptionObject}");
};

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddJwtToken(builder.Configuration);
builder.Services.AddServices(builder.Configuration);
builder.Services.AddSwagger();
builder.WebHost.UseKestrel()
               .UseUrls("http://*:5086");
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
    
    // Add shutdown handling
    app.Lifetime.ApplicationStopping.Register(() =>
    {
        Console.WriteLine("Application is stopping. Checking for reason...");
    });

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Application startup failed: {ex}");
    throw;
}
