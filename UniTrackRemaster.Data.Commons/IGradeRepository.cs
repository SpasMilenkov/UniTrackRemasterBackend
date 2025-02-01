using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Commons;

public interface IGradeRepository
{
    Task<Grade?> GetByIdAsync(Guid id);
    Task<IEnumerable<Grade>> GetAllAsync();
    Task<Grade> CreateAsync(Grade grade);
    Task UpdateAsync(Grade grade);
    Task DeleteAsync(Guid id);
}

