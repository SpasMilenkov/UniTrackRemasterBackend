using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Infrastructure;

public static class SwaggerExtensions
{
    public static void AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "UniTrack API",
                Description = "The ASP.NET Core Web API behind the UniTrack School Management System and analysis tool.",
                Contact = new OpenApiContact
                {
                    Name = "Spas Milenkov",
                    Email = "dummy@example.com",
                    Url = new Uri("https://github.com/SpasMilenkov")
                },
                Version = "v1"
            });
            
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Description = "Standard authorization header using bearer token",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });
            options.OperationFilter<SecurityRequirementsOperationFilter>();
            
        });
    }
}