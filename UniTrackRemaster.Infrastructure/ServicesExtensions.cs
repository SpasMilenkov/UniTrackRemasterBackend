using UniTrackRemaster.Mappings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniTrackRemaster.Data;
using UniTrackRemaster.Messaging;
using UniTrackRemaster.Services.Authentication;

namespace Infrastructure;

public static class ServicesExtensions
{
    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(c =>
        {
            c.AddPolicy("AllowOrigin",
                options => options.WithOrigins(
                        "https://localhost:5500",
                        "http://localhost:5173",
                        "http://localhost:4200",
                        "http://localhost")
                    .AllowCredentials()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
            );
        });
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMapper, Mapper>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
    }
}