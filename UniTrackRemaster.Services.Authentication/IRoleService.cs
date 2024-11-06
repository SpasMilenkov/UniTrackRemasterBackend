using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Services.Authentication;

public interface IRoleService
{
    Task<List<ApplicationRole>> GetAllRolesAsync();
    Task<List<ApplicationRole>> GetPublicRolesAsync();
}
