using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Commons.Repositories;

public interface IInstitutionRepository : IRepository<Institution>
{
    /// <summary>
    /// Gets an institution by ID with all related data
    /// </summary>
    /// <param name="id">Institution ID</param>
    /// <returns>Institution entity or null if not found</returns>
    Task<Institution?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets institutions associated with a specific user through various roles
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of institutions associated with the user</returns>
    Task<List<Institution>> GetInstitutionsByUserIdAsync(string userId);

    /// <summary>
    /// Gets all institutions (legacy method)
    /// </summary>
    /// <returns>List of all institutions</returns>
    Task<List<Institution>> GetAllAsync();

    /// <summary>
    /// Gets all institutions with optional filtering and pagination
    /// </summary>
    /// <param name="nameFilter">Optional name filter (case-insensitive partial match)</param>
    /// <param name="typeFilter">Optional institution type filter</param>
    /// <param name="locationFilter">Optional location type filter</param>
    /// <param name="integrationStatusFilter">Optional integration status filter</param>
    /// <param name="accreditationsFilter">Optional accreditations filter (comma-separated list)</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of institutions matching the filters</returns>
    Task<List<Institution>> GetAllAsync(string? nameFilter = null, string? typeFilter = null, 
        string? locationFilter = null, string? integrationStatusFilter = null, string? accreditationsFilter = null, 
        int page = 1, int pageSize = 50);

    /// <summary>
    /// Gets the total count of institutions matching the specified filters
    /// </summary>
    /// <param name="nameFilter">Optional name filter (case-insensitive partial match)</param>
    /// <param name="typeFilter">Optional institution type filter</param>
    /// <param name="locationFilter">Optional location type filter</param>
    /// <param name="integrationStatusFilter">Optional integration status filter</param>
    /// <param name="accreditationsFilter">Optional accreditations filter (comma-separated list)</param>
    /// <returns>Total count of institutions matching the filters</returns>
    Task<int> GetTotalCountAsync(string? nameFilter = null, string? typeFilter = null, 
        string? locationFilter = null, string? integrationStatusFilter = null, string? accreditationsFilter = null);

    /// <summary>
    /// Adds a new institution
    /// </summary>
    /// <param name="entity">Institution entity</param>
    /// <returns>Added institution entity</returns>
    Task<Institution> AddAsync(Institution entity);

    /// <summary>
    /// Updates an existing institution
    /// </summary>
    /// <param name="entity">Institution entity</param>
    Task UpdateAsync(Institution entity);

    /// <summary>
    /// Deletes an institution by ID
    /// </summary>
    /// <param name="id">Institution ID</param>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Checks if an institution exists by ID
    /// </summary>
    /// <param name="id">Institution ID</param>
    /// <returns>True if institution exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id);
}