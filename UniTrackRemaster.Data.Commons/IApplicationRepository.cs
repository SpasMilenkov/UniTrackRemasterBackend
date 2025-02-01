using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Commons;


public interface IApplicationRepository
{
    Task<Application?> GetByIdAsync(Guid id);
    Task<Application?> GetByInstitutionIdAsync(Guid id);
    Task<Application?> GetByEmailAsync(string email);
    Task<List<Application>> GetAllAsync();
    Task<Application> CreateAsync(Application application);
    Task<Application?> UpdateAsync(Guid id, Application updatedApplication);
    Task<Application?> ApproveAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
}
