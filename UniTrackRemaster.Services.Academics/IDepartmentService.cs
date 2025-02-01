using UniTrackRemaster.Api.Dto.Department;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;

namespace UniTrackRemaster.Services.Academics;

public interface IDepartmentService
{
    Task<DepartmentResponseDto> GetByIdAsync(Guid id);
    Task<IEnumerable<DepartmentResponseDto>> GetByFacultyAsync(Guid facultyId);
    Task<DepartmentResponseDto> CreateAsync(CreateDepartmentDto dto);
    Task<DepartmentResponseDto> UpdateAsync(Guid id, UpdateDepartmentDto dto);
    Task DeleteAsync(Guid id);
}