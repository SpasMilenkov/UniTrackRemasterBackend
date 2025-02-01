using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Commons.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Services.Authentication;

public class RoleService(
    RoleManager<ApplicationRole> roleManager,
    ILogger<RoleService> logger)
    : IRoleService
{
    public async Task<List<ApplicationRole>> GetAllRolesAsync()
    {
        try
        {
            return await roleManager.Roles.ToListAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving all roles");
            throw;
        }
    }

    public async Task<List<ApplicationRole>> GetPublicRolesAsync()
    {
        try
        {
            return await roleManager.Roles
                .Where(r => !new[] { nameof(Roles.SuperAdmin), nameof(Roles.Admin), nameof(Roles.Guest)  }
                    .Contains(r.Name))
                .ToListAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving public roles");
            throw;
        }
    }

    public async Task<bool> CreateRoleAsync(string roleName)
    {
        try
        {
            if (await roleManager.RoleExistsAsync(roleName))
                return false;

            var result = await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
            return result.Succeeded;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating role");
            throw;
        }
    }
}
