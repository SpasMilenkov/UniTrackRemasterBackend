using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using UniTrackRemaster.Data.Models.Users;
using UniTrackRemaster.Commons.Enums;

namespace UniTrackRemaster.Data.Seeding;

public class DataSeeder
{
    private static UserManager<ApplicationUser> _userManager = null!;
    public DataSeeder(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    }
    public static async Task SeedData(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        _userManager = userManager;
        
        await SeedRolesAsync(roleManager);
    }
    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        await EnsureRoleExistsAsync(roleManager, nameof(Roles.SuperAdmin));
        await EnsureRoleExistsAsync(roleManager, nameof(Roles.Admin));
        await EnsureRoleExistsAsync(roleManager, nameof(Roles.Guest));
        await EnsureRoleExistsAsync(roleManager, nameof(Roles.Teacher));
        await EnsureRoleExistsAsync(roleManager, nameof(Roles.Student));
        await EnsureRoleExistsAsync(roleManager, nameof(Roles.Parent));
    }

    private static async Task EnsureRoleExistsAsync(RoleManager<ApplicationRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new ApplicationRole(roleName));
        }
    }
}