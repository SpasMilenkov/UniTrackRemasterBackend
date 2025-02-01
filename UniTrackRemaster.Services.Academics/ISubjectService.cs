using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;

namespace UniTrackRemaster.Services.Academics;

public interface ISubjectService
{
    Task<SubjectResponseDto> GetByIdAsync(Guid id);
    Task<IEnumerable<SubjectResponseDto>> GetAllAsync();
    Task<SubjectResponseDto> CreateAsync(CreateSubjectDto dto);
    Task<SubjectResponseDto> UpdateAsync(Guid id, UpdateSubjectDto dto);
    Task DeleteAsync(Guid id);
}