using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Data.Models.TypeSafe;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Services.Authentication;

public class RoleService(RoleManager<ApplicationRole> roleManager) : IRoleService
{
    public async Task<List<ApplicationRole>> GetAllRolesAsync() => await roleManager.Roles.ToListAsync();
    
    public async Task<List<ApplicationRole>> GetPublicRolesAsync() => await roleManager.Roles
        .Where(r => r.Name != Ts.Roles.SuperAdmin && r.Name != Ts.Roles.Admin && r.Name != Ts.Roles.Guest)
        .ToListAsync();

}
