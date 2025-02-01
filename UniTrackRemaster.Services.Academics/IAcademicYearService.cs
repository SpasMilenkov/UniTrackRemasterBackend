using UniTrackRemaster.Api.Dto.AcademicYear;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;

namespace UniTrackRemaster.Services.Academics;

public interface IAcademicYearService
{
    Task<AcademicYearResponseDto> GetByIdAsync(Guid id);
    Task<IEnumerable<AcademicYearResponseDto>> GetByInstitutionAsync(Guid institutionId);
    Task<AcademicYearResponseDto> CreateAsync(CreateAcademicYearDto dto);
    Task<AcademicYearResponseDto> UpdateAsync(Guid id, UpdateAcademicYearDto dto);
    Task DeleteAsync(Guid id);
}