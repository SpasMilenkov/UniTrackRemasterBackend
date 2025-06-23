using UniTrackRemaster.Api.Dto.Institution;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Commons.Repositories;

/// <summary>
/// Repository interface for university operations
/// </summary>
public interface IUniversityRepository : IRepository<University>
{
    public Task<University> InitUniversityAsync(InitUniversityDto initDto);
    /// <summary>
    /// Get all universities with optional search query
    /// </summary>
    Task<IEnumerable<University>> GetAllAsync(string? searchQuery = null);

    /// <summary>
    /// Get university by ID
    /// </summary>
    Task<University?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get university by institution ID
    /// </summary>
    Task<University?> GetByInstitutionIdAsync(Guid institutionId);

    /// <summary>
    /// Create a new university
    /// </summary>
    Task<University> CreateAsync(University university);

    /// <summary>
    /// Update an existing university
    /// </summary>
    Task UpdateAsync(University university);

    /// <summary>
    /// Delete a university
    /// </summary>
    Task DeleteAsync(Guid id);
}