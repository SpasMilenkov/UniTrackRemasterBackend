using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Parent;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Services.User.Parents.Exceptions;

namespace UniTrackRemaster.Services.User.Parents;

/// <summary>
/// Service implementation for parent management operations
/// </summary>
public class ParentService : IParentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ParentService> _logger;

    public ParentService(
        IUnitOfWork unitOfWork,
        ILogger<ParentService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ParentResponseDto> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving parent with ID: {ParentId}", id);

        try
        {
            var parent = await _unitOfWork.Parents.GetByIdAsync(id);

            if (parent == null)
            {
                _logger.LogWarning("Parent not found with ID: {ParentId}", id);
                throw new ParentNotFoundException(id);
            }

            if (parent.User == null)
            {
                _logger.LogError("Parent {ParentId} exists but has no associated user", id);
                throw new ParentBusinessRuleViolationException("DataIntegrity",
                    $"Parent {id} exists but has no associated user");
            }

            _logger.LogDebug("Successfully retrieved parent: {ParentId}", id);
            return ParentResponseDto.FromEntity(parent);
        }
        catch (Exception ex) when (ex is not ParentException)
        {
            _logger.LogError(ex, "Unexpected error retrieving parent: {ParentId}", id);
            throw;
        }
    }

    public async Task<ParentResponseDto> GetByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Retrieving parent for user: {UserId}", userId);

        try
        {
            var parent = await _unitOfWork.Parents.GetByUserIdAsync(userId);

            if (parent == null)
            {
                _logger.LogWarning("No parent found for user: {UserId}", userId);
                throw new ParentNotFoundException($"user ID: {userId}");
            }

            if (parent.User == null)
            {
                _logger.LogError("Parent {ParentId} exists but has no associated user", parent.Id);
                throw new ParentBusinessRuleViolationException("DataIntegrity",
                    $"Parent {parent.Id} exists but has no associated user");
            }

            _logger.LogDebug("Successfully retrieved parent for user: {UserId}", userId);
            return ParentResponseDto.FromEntity(parent);
        }
        catch (Exception ex) when (ex is not ParentException)
        {
            _logger.LogError(ex, "Unexpected error retrieving parent for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<PagedResult<ParentResponseDto>> GetAllAsync(string? statusFilter = null, int page = 1,
        int pageSize = 50)
    {
        _logger.LogInformation("Retrieving parents with filter: {StatusFilter}, Page: {Page}, PageSize: {PageSize}",
            statusFilter, page, pageSize);

        // Validate pagination parameters
        if (page < 1)
        {
            throw new ArgumentException("Page number must be greater than 0", nameof(page));
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new ArgumentException("Page size must be between 1 and 100", nameof(pageSize));
        }

        try
        {
            var parents = await _unitOfWork.Parents.GetAllAsync(statusFilter, page, pageSize);
            var totalCount = await _unitOfWork.Parents.GetTotalCountAsync(statusFilter);

            var parentDtos = parents
                .Where(p => p.User != null)
                .Select(ParentResponseDto.FromEntity)
                .ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            _logger.LogDebug("Successfully retrieved {Count} parents (Page {Page} of {TotalPages})",
                parentDtos.Count, page, totalPages);

            return new PagedResult<ParentResponseDto>
            {
                Items = parentDtos,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
            };
        }
        catch (Exception ex) when (ex is not ParentException and not ArgumentException)
        {
            _logger.LogError(ex, "Unexpected error retrieving parents");
            throw;
        }
    }

    public async Task<List<ChildResponseDto>> GetChildrenAsync(Guid parentId)
    {
        _logger.LogInformation("Retrieving children for parent: {ParentId}", parentId);

        try
        {
            // First verify parent exists
            var parent = await _unitOfWork.Parents.GetByIdAsync(parentId);
            if (parent == null)
            {
                throw new ParentNotFoundException(parentId);
            }

            var children = await _unitOfWork.Parents.GetChildrenAsync(parentId);

            var childDtos = children
                .Where(c => c.User != null)
                .Select(ChildResponseDto.FromEntity)
                .ToList();

            _logger.LogDebug("Successfully retrieved {Count} children for parent: {ParentId}",
                childDtos.Count, parentId);

            return childDtos;
        }
        catch (Exception ex) when (ex is not ParentException)
        {
            _logger.LogError(ex, "Unexpected error retrieving children for parent: {ParentId}", parentId);
            throw;
        }
    }

    public async Task<ParentResponseDto> CreateAsync(CreateParentDto createParentDto)
    {
        _logger.LogInformation("Creating parent for user: {UserId}", createParentDto.UserId);

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var parentEntity = CreateParentDto.ToEntity(createParentDto);
            var createdParent = await _unitOfWork.Parents.CreateAsync(parentEntity);

            // Reload with full data
            var parent = await _unitOfWork.Parents.GetByIdAsync(createdParent.Id);

            if (parent?.User == null)
            {
                throw new ParentBusinessRuleViolationException("DataIntegrity",
                    "Failed to create parent with proper user association");
            }

            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Successfully created parent: {ParentId}", parent.Id);

            return ParentResponseDto.FromEntity(parent);
        }
        catch (ParentException)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Unexpected error creating parent for user: {UserId}", createParentDto.UserId);
            throw;
        }
    }

    public async Task<ParentResponseDto> UpdateAsync(Guid id, UpdateParentDto updateParentDto)
    {
        _logger.LogInformation("Updating parent: {ParentId}", id);

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var parent = await _unitOfWork.Parents.UpdateAsync(id, UpdateParentDto.ToEntity(updateParentDto));

            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Successfully updated parent: {ParentId}", id);

            return ParentResponseDto.FromEntity(parent);
        }
        catch (ParentException)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Unexpected error updating parent: {ParentId}", id);
            throw;
        }
    }

    public async Task<ParentResponseDto> UpdateStatusAsync(Guid id, ProfileStatus status)
    {
        _logger.LogInformation("Updating parent status: {ParentId} to {Status}", id, status);

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var parent = await _unitOfWork.Parents.UpdateStatusAsync(id, status);

            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Successfully updated parent status: {ParentId} to {Status}", id, status);

            return ParentResponseDto.FromEntity(parent);
        }
        catch (ParentException)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Unexpected error updating parent status: {ParentId}", id);
            throw;
        }
    }

    public async Task<ParentResponseDto> AddChildAsync(Guid parentId, AddChildDto addChildDto)
    {
        _logger.LogInformation("Adding child {StudentId} to parent: {ParentId}", addChildDto.StudentId, parentId);

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var parent = await _unitOfWork.Parents.AddChildAsync(parentId, addChildDto.StudentId);

            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Successfully added child {StudentId} to parent: {ParentId}",
                addChildDto.StudentId, parentId);

            return ParentResponseDto.FromEntity(parent);
        }
        catch (ParentException)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Unexpected error adding child {StudentId} to parent: {ParentId}",
                addChildDto.StudentId, parentId);
            throw;
        }
    }

    public async Task<ParentResponseDto> RemoveChildAsync(Guid parentId, Guid studentId)
    {
        _logger.LogInformation("Removing child {StudentId} from parent: {ParentId}", studentId, parentId);

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var parent = await _unitOfWork.Parents.RemoveChildAsync(parentId, studentId);

            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Successfully removed child {StudentId} from parent: {ParentId}",
                studentId, parentId);

            return ParentResponseDto.FromEntity(parent);
        }
        catch (ParentException)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Unexpected error removing child {StudentId} from parent: {ParentId}",
                studentId, parentId);
            throw;
        }
    }

    public async Task<ParentResponseDto> ActivateAsync(Guid id)
    {
        _logger.LogInformation("Activating parent: {ParentId}", id);

        return await UpdateStatusAsync(id, ProfileStatus.Active);
    }

    public async Task<ParentResponseDto> SuspendAsync(Guid id)
    {
        _logger.LogInformation("Suspending parent: {ParentId}", id);

        return await UpdateStatusAsync(id, ProfileStatus.Suspended);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting parent: {ParentId}", id);

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var parent = await _unitOfWork.Parents.GetByIdAsync(id);
            if (parent == null)
            {
                _logger.LogWarning("Parent not found for deletion: {ParentId}", id);
                throw new ParentNotFoundException(id);
            }

            var result = await _unitOfWork.Parents.DeleteAsync(id);

            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Successfully deleted parent: {ParentId}", id);

            return result;
        }
        catch (ParentException)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Unexpected error deleting parent: {ParentId}", id);
            throw;
        }
    }
}