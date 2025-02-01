using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Commons;

public interface IFacultyRepository
{
    Task<Faculty?> GetByIdAsync(Guid id);
    Task<IEnumerable<Faculty>> GetByUniversityAsync(Guid universityId);
    Task<Faculty> CreateAsync(Faculty faculty);
    Task UpdateAsync(Faculty faculty);
    Task DeleteAsync(Guid id);
}
