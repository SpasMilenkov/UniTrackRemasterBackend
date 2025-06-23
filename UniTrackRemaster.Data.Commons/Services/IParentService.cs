using UniTrackRemaster.Api.Dto.Parent;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Commons.Services;


/// <summary>
/// Service interface for parent management operations
/// </summary>
public interface IParentService
{
    /// <summary>
    /// Gets a parent by their ID
    /// </summary>
    /// <param name="id">Parent ID</param>
    /// <returns>Parent details</returns>
    Task<ParentResponseDto> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets a parent by their user ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Parent details</returns>
    Task<ParentResponseDto> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Gets all parents with optional filtering and pagination
    /// </summary>
    /// <param name="statusFilter">Optional status filter</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paginated list of parents</returns>
    Task<PagedResult<ParentResponseDto>> GetAllAsync(string? statusFilter = null, int page = 1, int pageSize = 50);

    /// <summary>
    /// Gets children of a specific parent
    /// </summary>
    /// <param name="parentId">Parent ID</param>
    /// <returns>List of children</returns>
    Task<List<ChildResponseDto>> GetChildrenAsync(Guid parentId);

    /// <summary>
    /// Creates a new parent profile
    /// </summary>
    /// <param name="createParentDto">Parent creation data</param>
    /// <returns>Created parent details</returns>
    Task<ParentResponseDto> CreateAsync(CreateParentDto createParentDto);

    /// <summary>
    /// Updates an existing parent profile
    /// </summary>
    /// <param name="id">Parent ID</param>
    /// <param name="updateParentDto">Updated parent data</param>
    /// <returns>Updated parent details</returns>
    Task<ParentResponseDto> UpdateAsync(Guid id, UpdateParentDto updateParentDto);

    /// <summary>
    /// Updates parent status
    /// </summary>
    /// <param name="id">Parent ID</param>
    /// <param name="status">New status</param>
    /// <returns>Updated parent details</returns>
    Task<ParentResponseDto> UpdateStatusAsync(Guid id, ProfileStatus status);

    /// <summary>
    /// Adds a child to a parent
    /// </summary>
    /// <param name="parentId">Parent ID</param>
    /// <param name="addChildDto">Child addition data</param>
    /// <returns>Updated parent details</returns>
    Task<ParentResponseDto> AddChildAsync(Guid parentId, AddChildDto addChildDto);

    /// <summary>
    /// Removes a child from a parent
    /// </summary>
    /// <param name="parentId">Parent ID</param>
    /// <param name="studentId">Student ID to remove</param>
    /// <returns>Updated parent details</returns>
    Task<ParentResponseDto> RemoveChildAsync(Guid parentId, Guid studentId);

    /// <summary>
    /// Activates a parent profile
    /// </summary>
    /// <param name="id">Parent ID</param>
    /// <returns>Activated parent details</returns>
    Task<ParentResponseDto> ActivateAsync(Guid id);

    /// <summary>
    /// Suspends a parent profile
    /// </summary>
    /// <param name="id">Parent ID</param>
    /// <returns>Suspended parent details</returns>
    Task<ParentResponseDto> SuspendAsync(Guid id);

    /// <summary>
    /// Deletes a parent profile (soft delete)
    /// </summary>
    /// <param name="id">Parent ID</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteAsync(Guid id);
}