using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Commons.Repositories;


public interface IApplicationRepository : IRepository<Application>
{
    Task<Application?> GetByIdAsync(Guid id);
    Task<Application?> GetByInstitutionIdAsync(Guid id);
    Task<Application?> GetByEmailAsync(string email);
    Task<List<Application>> GetAllAsync(string? statusFilter = null, int page = 1, int pageSize = 50);
    Task<int> GetTotalCountAsync(string? statusFilter = null);
    Task<bool> ExistsByEmailAsync(string email);
    Task<Application> CreateAsync(Application application);
    Task<Application> UpdateAsync(Guid id, Application updatedApplication);
    Task<Application> ApproveAsync(Guid id);
    Task<Application> RejectAsync(Guid id, string? reason = null);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> IsCodeValidAsync(string email, string code);
    Task<Application> VerifyCodeAsync(string email, string code);
}
