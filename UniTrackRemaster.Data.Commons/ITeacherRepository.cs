using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Commons;

public interface ITeacherRepository
{
    Task<Teacher?> GetByIdAsync(Guid id);
    Task<IEnumerable<Teacher>> GetAllAsync();
    Task<Teacher> CreateAsync(Teacher teacher);
    Task UpdateAsync(Teacher teacher);
    Task DeleteAsync(Guid id);
    Task<Teacher?> GetByUserIdAsync(Guid userId);
}
