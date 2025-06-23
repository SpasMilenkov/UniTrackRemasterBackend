using Microsoft.AspNetCore.Http;
using UniTrackRemaster.Api.Dto.Institution;

namespace UniTrackRemaster.Commons.Services;

public interface IInstitutionService
{
    /// <summary>
    /// Gets an institution by ID with signed URLs for images and logo
    /// </summary>
    /// <param name="id">Institution ID</param>
    /// <returns>Institution details</returns>
    Task<InstitutionDto> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all institutions (legacy method - consider using paginated version for large datasets)
    /// </summary>
    /// <returns>List of all institutions</returns>
    Task<List<InstitutionDto>> GetAllAsync();

    /// <summary>
    /// Gets all institutions with optional filtering and pagination
    /// </summary>
    /// <param name="nameFilter">Optional name filter (case-insensitive partial match)</param>
    /// <param name="typeFilter">Optional institution type filter</param>
    /// <param name="locationFilter">Optional location type filter</param>
    /// <param name="integrationStatusFilter">Optional integration status filter</param>
    /// <param name="accreditationsFilter">Optional accreditations filter (comma-separated list)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50, max: 100)</param>
    /// <returns>Paginated list of institutions matching the filters</returns>
    Task<PagedResult<InstitutionDto>> GetAllAsync(string? nameFilter = null, string? typeFilter = null, 
        string? locationFilter = null, string? integrationStatusFilter = null, string? accreditationsFilter = null, 
        int page = 1, int pageSize = 50);

    /// <summary>
    /// Updates an institution with optional logo and image uploads
    /// </summary>
    /// <param name="id">Institution ID</param>
    /// <param name="updateDto">Updated institution data</param>
    /// <param name="logo">Optional new logo file</param>
    /// <param name="newImages">Optional new image files</param>
    Task UpdateAsync(Guid id, UpdateInstitutionDto updateDto, IFormFile? logo = null, IEnumerable<IFormFile>? newImages = null);

    /// <summary>
    /// Gets institutions associated with a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of institutions associated with the user</returns>
    Task<List<InstitutionDto>> GetInstitutionsByUserId(string userId);

    /// <summary>
    /// Deletes an institution and all associated files
    /// </summary>
    /// <param name="id">Institution ID</param>
    Task DeleteAsync(Guid id);
}