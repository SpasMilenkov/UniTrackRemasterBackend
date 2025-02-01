using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Commons;

public interface IMajorRepository
{
    Task<Major?> GetByIdAsync(Guid id);
    Task<IEnumerable<Major>> GetByFacultyAsync(Guid facultyId);
    Task<Major> CreateAsync(Major major);
    Task UpdateAsync(Major major);
    Task DeleteAsync(Guid id);
}
