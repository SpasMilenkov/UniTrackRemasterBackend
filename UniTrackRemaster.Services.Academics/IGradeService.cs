using UniTrackRemaster.Api.Dto.Grade;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;

namespace UniTrackRemaster.Services.Academics;

public interface IGradeService
{
    Task<GradeResponseDto> GetByIdAsync(Guid id);
    Task<IEnumerable<GradeResponseDto>> GetAllAsync();
    Task<GradeResponseDto> CreateAsync(CreateGradeDto dto);
    Task<GradeResponseDto> UpdateAsync(Guid id, UpdateGradeDto dto);
    Task DeleteAsync(Guid id);
}
