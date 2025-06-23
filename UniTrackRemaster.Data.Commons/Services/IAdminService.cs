using UniTrackRemaster.Api.Dto.Admin;

namespace UniTrackRemaster.Commons.Services;

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