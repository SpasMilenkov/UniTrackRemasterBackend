using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Users;

namespace Infrastructure;

public static class IdentityServicesExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<UniTrackDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database")));

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {

                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<UniTrackDbContext>();
        
        return services;
    }
}