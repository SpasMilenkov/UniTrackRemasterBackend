using UniTrackRemaster.Api.Dto.Major;

namespace UniTrackRemaster.Commons.Services;

public interface IMajorService
{
    Task<MajorResponseDto> GetByIdAsync(Guid id);
    Task<IEnumerable<MajorResponseDto>> GetByFacultyAsync(Guid facultyId);
    Task<MajorResponseDto> CreateAsync(CreateMajorDto dto);
    Task<MajorResponseDto> UpdateAsync(Guid id, UpdateMajorDto dto);
    Task DeleteAsync(Guid id);
}