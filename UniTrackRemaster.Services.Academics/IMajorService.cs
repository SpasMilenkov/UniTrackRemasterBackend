using UniTrackRemaster.Api.Dto.Major;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Services.Academics;

public interface IMajorService
{
    Task<MajorResponseDto> GetByIdAsync(Guid id);
    Task<IEnumerable<MajorResponseDto>> GetByFacultyAsync(Guid facultyId);
    Task<MajorResponseDto> CreateAsync(CreateMajorDto dto);
    Task<MajorResponseDto> UpdateAsync(Guid id, UpdateMajorDto dto);
    Task DeleteAsync(Guid id);
}