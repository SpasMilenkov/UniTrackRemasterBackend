using UniTrackRemaster.Mappings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrganizationServices;
using StorageService;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data;
using UniTrackRemaster.Data.Repositories;
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
                        "https://unitrack.io:8080",
                        "http://localhost:3000",
                        "http://localhost:8080")
                    .AllowCredentials()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
            );
        });
        //TODO: one day I might convert the repos to the UniOfWork pattern, not today though
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMapper, Mapper>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<ISchoolImageRepository, SchoolImageRepository>();
        services.AddScoped<ISchoolImageService, SchoolImageService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<ISchoolRepository, SchoolRepository>();
        services.AddScoped<ISchoolService, SchoolService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddSingleton<IFirebaseStorageService>(provider => 
            new FirebaseStorageService(
                configuration.GetSection("FirebaseCredentials").GetSection("CredentialsPath").Value ?? string.Empty,
                configuration.GetSection("FirebaseCredentials").GetSection("BucketPath").Value ?? string.Empty
            ));
    }
}