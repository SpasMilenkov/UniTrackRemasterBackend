using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Commons;

public interface IInstitutionRepository
{
    Task<Institution?> GetByIdAsync(Guid id);
    Task<List<Institution>> GetAllAsync();
    Task<Institution> AddAsync(Institution entity);
    Task UpdateAsync(Institution entity);
    Task<List<Institution>> GetInstitutionsByUserIdAsync(string userId);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}