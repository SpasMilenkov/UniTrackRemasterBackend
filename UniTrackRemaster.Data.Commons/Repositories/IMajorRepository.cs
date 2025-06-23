using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Commons.Repositories;

public interface IMajorRepository: IRepository<Major>
{
    Task<Major?> GetByIdAsync(Guid id);
    Task<IEnumerable<Major>> GetByFacultyAsync(Guid facultyId);
    Task<Major> CreateAsync(Major major);
    Task UpdateAsync(Major major);
    Task DeleteAsync(Guid id);
}
