using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Commons.Repositories;

/// <summary>
/// Repository interface for parent operations
/// </summary>
public interface IParentRepository : IRepository<Parent>
{
    /// <summary>
    /// Gets a parent by their ID with all navigation properties
    /// </summary>
    /// <param name="id">Parent ID</param>
    /// <returns>Parent with user and children information, or null if not found</returns>
    Task<Parent?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets a parent by their user ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Parent associated with the user, or null if not found</returns>
    Task<Parent?> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Gets all parents with optional filtering and pagination
    /// </summary>
    /// <param name="statusFilter">Optional status filter</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of parents</returns>
    Task<List<Parent>> GetAllAsync(string? statusFilter = null, int page = 1, int pageSize = 50);

    /// <summary>
    /// Gets total count of parents with optional filtering
    /// </summary>
    /// <param name="statusFilter">Optional status filter</param>
    /// <returns>Total count</returns>
    Task<int> GetTotalCountAsync(string? statusFilter = null);

    /// <summary>
    /// Gets children of a specific parent
    /// </summary>
    /// <param name="parentId">Parent ID</param>
    /// <returns>List of students who are children of the parent</returns>
    Task<List<Student>> GetChildrenAsync(Guid parentId);

    /// <summary>
    /// Checks if a parent profile already exists for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>True if parent exists, false otherwise</returns>
    Task<bool> ExistsByUserIdAsync(Guid userId);

    /// <summary>
    /// Checks if a student is already a child of a parent
    /// </summary>
    /// <param name="parentId">Parent ID</param>
    /// <param name="studentId">Student ID</param>
    /// <returns>True if the student is a child of the parent</returns>
    Task<bool> IsChildOfParentAsync(Guid parentId, Guid studentId);

    /// <summary>
    /// Gets the number of parents a student has
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <returns>Count of parents</returns>
    Task<int> GetParentCountForStudentAsync(Guid studentId);

    /// <summary>
    /// Creates a new parent profile
    /// </summary>
    /// <param name="parent">Parent entity to create</param>
    /// <returns>Created parent</returns>
    Task<Parent> CreateAsync(Parent parent);

    /// <summary>
    /// Updates an existing parent profile
    /// </summary>
    /// <param name="id">Parent ID</param>
    /// <param name="updatedParent">Updated parent data</param>
    /// <returns>Updated parent</returns>
    Task<Parent> UpdateAsync(Guid id, Parent updatedParent);

    /// <summary>
    /// Updates parent status
    /// </summary>
    /// <param name="id">Parent ID</param>
    /// <param name="status">New status</param>
    /// <returns>Updated parent</returns>
    Task<Parent> UpdateStatusAsync(Guid id, ProfileStatus status);

    /// <summary>
    /// Adds a child to a parent
    /// </summary>
    /// <param name="parentId">Parent ID</param>
    /// <param name="studentId">Student ID</param>
    /// <returns>Updated parent with new child</returns>
    Task<Parent> AddChildAsync(Guid parentId, Guid studentId);

    /// <summary>
    /// Removes a child from a parent
    /// </summary>
    /// <param name="parentId">Parent ID</param>
    /// <param name="studentId">Student ID</param>
    /// <returns>Updated parent without the child</returns>
    Task<Parent> RemoveChildAsync(Guid parentId, Guid studentId);

    /// <summary>
    /// Soft deletes a parent by setting status to inactive
    /// </summary>
    /// <param name="id">Parent ID</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteAsync(Guid id);
}