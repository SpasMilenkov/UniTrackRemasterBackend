using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Commons.Repositories;

public interface IAdminRepository : IRepository<Admin>
{
    Task<IEnumerable<Admin>> GetAllAsync();
    Task<Admin?> GetByIdAsync(Guid id);
    Task<Admin?> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Admin>> GetByInstitutionAsync(Guid institutionId);
    Task<Admin> CreateAsync(Admin admin);
    Task UpdateAsync(Admin admin);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> HasActiveAdminAsync(Guid institutionId);
    Task<IEnumerable<Admin>> GetPendingByUserIdAsync(Guid userId);
    Task<IEnumerable<Admin>> GetByInstitutionAsync(Guid institutionId, ProfileStatus? status = null);
    Task<IEnumerable<Admin>> GetByStatusAsync(ProfileStatus status);
}