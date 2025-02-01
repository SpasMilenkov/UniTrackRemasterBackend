using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;

namespace UniTrackRemaster.Services.Admin;

public interface IAdminService
{
    Task<IEnumerable<AdminDto>> GetAllAsync();
    Task<AdminDto?> GetByIdAsync(Guid id);
    Task<AdminDto?> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<AdminDto>> GetByInstitutionAsync(Guid institutionId);
    Task<AdminDto> CreateAsync(CreateAdminDto createAdminDto);
    Task<AdminDto> UpdateAsync(Guid id, UpdateAdminDto updateAdminDto);
    Task DeleteAsync(Guid id);
    
}