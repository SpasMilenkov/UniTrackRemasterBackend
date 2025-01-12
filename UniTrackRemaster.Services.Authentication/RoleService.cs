using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Commons.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Services.Authentication;

public class RoleService : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        RoleManager<ApplicationRole> roleManager,
        ILogger<RoleService> logger)
    {
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<List<ApplicationRole>> GetAllRolesAsync()
    {
        try
        {
            return await _roleManager.Roles.ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving all roles");
            throw;
        }
    }

    public async Task<List<ApplicationRole>> GetPublicRolesAsync()
    {
        try
        {
            return await _roleManager.Roles
                .Where(r => !new[] { nameof(Roles.SuperAdmin), nameof(Roles.Admin), nameof(Roles.Guest)  }
                    .Contains(r.Name))
                .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving public roles");
            throw;
        }
    }

    public async Task<bool> CreateRoleAsync(string roleName)
    {
        try
        {
            if (await _roleManager.RoleExistsAsync(roleName))
                return false;

            var result = await _roleManager.CreateAsync(new ApplicationRole { Name = roleName });
            return result.Succeeded;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating role");
            throw;
        }
    }
}
