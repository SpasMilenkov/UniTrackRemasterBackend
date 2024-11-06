using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UniTrackRemaster.Data.Context;

namespace Infrastructure;

public static class MigrationBuilder
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UniTrackDbContext>();
        dbContext.Database.Migrate();
    }
}