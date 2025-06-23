using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Commons.Services;

public interface IRoleService
{
    Task<List<ApplicationRole>> GetAllRolesAsync();
    Task<List<ApplicationRole>> GetPublicRolesAsync();
    Task<List<string>> GetUserRolesInInstitutionAsync(Guid userId, Guid institutionId);
    Task<bool> IsUserInRole(Guid userId, string roleName);
}
