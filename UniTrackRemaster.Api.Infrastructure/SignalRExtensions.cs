using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure;

public static class SignalRExtensions
{
    public static void ConfigureSignalR(this IServiceCollection services, ConfigurationManager configuration, IHostEnvironment env)
    {
        services.AddSignalR(options =>
        {

            options.EnableDetailedErrors = env.IsDevelopment();
            options.MaximumReceiveMessageSize = 102400; // 100KB
            options.StreamBufferCapacity = 10;
            options.KeepAliveInterval = TimeSpan.FromSeconds(10);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
        });
    }
}
