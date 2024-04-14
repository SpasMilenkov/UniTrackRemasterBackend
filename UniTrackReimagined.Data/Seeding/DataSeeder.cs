using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using UniTrackReimagined.Data.Models.TypeSafe;
using UniTrackReimagined.Data.Models.Users;

namespace UniTrackReimagined.Data.Seeding;

public class DataSeeder
{
    private static UserManager<ApplicationUser> _userManager = null!;
    public DataSeeder(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    }
    public static async Task SeedData(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        _userManager = userManager;
        
        await SeedRolesAsync(roleManager);
    }
    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        await EnsureRoleExistsAsync(roleManager, Ts.Roles.SuperAdmin);
        await EnsureRoleExistsAsync(roleManager, Ts.Roles.Admin);
        await EnsureRoleExistsAsync(roleManager, Ts.Roles.Guest);
        await EnsureRoleExistsAsync(roleManager, Ts.Roles.Teacher);
        await EnsureRoleExistsAsync(roleManager, Ts.Roles.Student);
        await EnsureRoleExistsAsync(roleManager, Ts.Roles.Parent);
    }

    private static async Task EnsureRoleExistsAsync(RoleManager<ApplicationRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new ApplicationRole(roleName));
        }
    }
}