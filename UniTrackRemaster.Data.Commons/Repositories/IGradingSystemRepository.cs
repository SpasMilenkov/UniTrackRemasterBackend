using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Commons.Repositories;

public interface IGradingSystemRepository
{
    Task<GradingSystem> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<GradingSystem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<GradingSystem> GetDefaultForInstitutionAsync(Guid institutionId, CancellationToken cancellationToken = default);
    Task<List<GradingSystem>> GetAllForInstitutionAsync(Guid institutionId, CancellationToken cancellationToken = default);
    Task<GradingSystem> GetWithGradeScalesAsync(Guid gradingSystemId, CancellationToken cancellationToken = default);
    Task<bool> SetDefaultAsync(Guid gradingSystemId, Guid institutionId, CancellationToken cancellationToken = default);
    Task<GradingSystem> AddAsync(GradingSystem gradingSystem, CancellationToken cancellationToken = default);
    Task<GradingSystem> UpdateAsync(GradingSystem gradingSystem, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GradingSystem> GetByNameAndInstitutionAsync(string name, Guid institutionId, CancellationToken cancellationToken = default);
}