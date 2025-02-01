using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;

namespace UniTrackRemaster.Services.Academics;

public interface ITeacherService
{
    Task<TeacherResponseDto> GetByIdAsync(Guid id);
    Task<IEnumerable<TeacherResponseDto>> GetAllAsync();
    Task<TeacherResponseDto> CreateAsync(CreateTeacherDto dto);
    Task<TeacherResponseDto> UpdateAsync(Guid id, UpdateTeacherDto dto);
    Task DeleteAsync(Guid id);
}