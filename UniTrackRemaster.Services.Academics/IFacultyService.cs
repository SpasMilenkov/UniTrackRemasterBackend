using UniTrackRemaster.Api.Dto.Faculty;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;

namespace UniTrackRemaster.Services.Academics;

public interface IFacultyService
{
    Task<FacultyResponseDto> GetByIdAsync(Guid id);
    Task<IEnumerable<FacultyResponseDto>> GetByUniversityAsync(Guid universityId);
    Task<FacultyResponseDto> CreateAsync(CreateFacultyDto dto);
    Task<FacultyResponseDto> UpdateAsync(Guid id, UpdateFacultyDto dto);
    Task DeleteAsync(Guid id);
}
