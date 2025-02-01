using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Commons;

public interface ISubjectRepository
{
    Task<Subject?> GetByIdAsync(Guid id);
    Task<IEnumerable<Subject>> GetAllAsync();
    Task<Subject> CreateAsync(Subject subject);
    Task UpdateAsync(Subject subject);
    Task DeleteAsync(Guid id);
}