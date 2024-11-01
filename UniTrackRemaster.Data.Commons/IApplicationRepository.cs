using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Commons;

public interface IApplicationRepository
{
    public Task<Application?> GetApplicationByIdAsync(Guid id);
    public Task<List<Application>> GetAllApplicationsAsync();
    public Task<Application> CreateApplicationAsync(Application application);
    public Task<Application?> UpdateApplicationAsync(Guid id, Application updatedApplication);
    public Task<bool> DeleteApplicationAsync(Guid id);
}