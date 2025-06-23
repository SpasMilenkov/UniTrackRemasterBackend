using Microsoft.AspNetCore.Http;
using UniTrackRemaster.Api.Dto.Institution;
using UniTrackRemaster.Api.Dto.University;

namespace UniTrackRemaster.Commons.Services;

/// <summary>
/// Service interface for university operations
/// </summary>
public interface IUniversityService
{
    Task<Guid> InitUniversityAsync(InitUniversityDto universityData, IFormFile? logo, List<IFormFile> images);
    
    /// <summary>
    /// Get all universities with optional search query
    /// </summary>
    Task<IEnumerable<UniversityResponseDto>> GetAllAsync(string? searchQuery = null);
    
    /// <summary>
    /// Get university by ID
    /// </summary>
    Task<UniversityResponseDto> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Get university by institution ID
    /// </summary>
    Task<UniversityResponseDto> GetByInstitutionIdAsync(Guid institutionId);
    
    /// <summary>
    /// Update an existing university
    /// </summary>
    Task<UniversityResponseDto> UpdateAsync(Guid id, UpdateUniversityDto dto);
    
    /// <summary>
    /// Delete a university
    /// </summary>
    Task DeleteAsync(Guid id);
}