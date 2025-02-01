using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Commons;

public interface IAdminRepository
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
}