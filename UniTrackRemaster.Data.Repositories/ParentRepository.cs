using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;
using UniTrackRemaster.Services.User.Parents.Exceptions;

namespace UniTrackRemaster.Data.Repositories;

/// <summary>
/// Repository implementation for parent operations
/// </summary>
public class ParentRepository : Repository<Parent>, IParentRepository
{
    public ParentRepository(UniTrackDbContext context) : base(context) { }

    public async Task<Parent?> GetByIdAsync(Guid id) => 
        await _context.Parents
            .Include(p => p.User)
            .Include(p => p.Children)
                .ThenInclude(c => c.User)
            .Include(p => p.Children)
                .ThenInclude(c => c.Grade)
            .Include(p => p.Children)
                .ThenInclude(c => c.Major)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Parent?> GetByUserIdAsync(Guid userId) => 
        await _context.Parents
            .Include(p => p.User)
            .Include(p => p.Children)
                .ThenInclude(c => c.User)
            .Include(p => p.Children)
                .ThenInclude(c => c.Grade)
            .Include(p => p.Children)
                .ThenInclude(c => c.Major)
            .FirstOrDefaultAsync(p => p.UserId == userId);

    public async Task<List<Parent>> GetAllAsync(string? statusFilter = null, int page = 1, int pageSize = 50)
    {
        var query = _context.Parents
            .Include(p => p.User)
            .Include(p => p.Children)
                .ThenInclude(c => c.User)
            .Include(p => p.Children)
                .ThenInclude(c => c.Grade)
            .Include(p => p.Children)
                .ThenInclude(c => c.Major)
            .AsQueryable();

        // Apply status filter if provided
        if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<ProfileStatus>(statusFilter, true, out var status))
        {
            query = query.Where(p => p.Status == status);
        }

        // Apply pagination
        return await query
            .OrderBy(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(string? statusFilter = null)
    {
        var query = _context.Parents.AsQueryable();

        if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<ProfileStatus>(statusFilter, true, out var status))
        {
            query = query.Where(p => p.Status == status);
        }

        return await query.CountAsync();
    }

    public async Task<List<Student>> GetChildrenAsync(Guid parentId)
    {
        var parent = await _context.Parents
            .Include(p => p.Children)
                .ThenInclude(c => c.User)
            .Include(p => p.Children)
                .ThenInclude(c => c.Grade)
            .Include(p => p.Children)
                .ThenInclude(c => c.Major)
            .FirstOrDefaultAsync(p => p.Id == parentId);

        return parent?.Children?.ToList() ?? new List<Student>();
    }

    public async Task<bool> ExistsByUserIdAsync(Guid userId)
    {
        return await _context.Parents.AnyAsync(p => p.UserId == userId);
    }

    public async Task<bool> IsChildOfParentAsync(Guid parentId, Guid studentId)
    {
        return await _context.Parents
            .Where(p => p.Id == parentId)
            .SelectMany(p => p.Children)
            .AnyAsync(c => c.Id == studentId);
    }

    public async Task<int> GetParentCountForStudentAsync(Guid studentId)
    {
        return await _context.Parents
            .Where(p => p.Children.Any(c => c.Id == studentId))
            .CountAsync();
    }

    public async Task<Parent> CreateAsync(Parent parent)
    {
        // Validate business rules
        if (await ExistsByUserIdAsync(parent.UserId))
        {
            throw new DuplicateParentException(parent.UserId);
        }

        // Validate that the user exists
        var userExists = await _context.Users.AnyAsync(u => u.Id == parent.UserId);
        if (!userExists)
        {
            throw new ParentBusinessRuleViolationException("UserValidation", 
                $"User with ID {parent.UserId} does not exist");
        }

        // Add to context (don't save - that's handled by UnitOfWork)
        await _context.Parents.AddAsync(parent);
        return parent;
    }

    public async Task<Parent> UpdateAsync(Guid id, Parent updatedParent)
    {
        var parent = await GetByIdAsync(id);
        
        if (parent == null) 
        {
            throw new ParentNotFoundException(id);
        }

        // Update allowed properties
        parent.Occupation = updatedParent.Occupation;
        parent.EmergencyContact = updatedParent.EmergencyContact;
        parent.Notes = updatedParent.Notes;
        
        // Only update status if it's provided and different
        if (updatedParent.Status != ProfileStatus.Pending || parent.Status != ProfileStatus.Pending)
        {
            parent.Status = updatedParent.Status;
        }
        
        parent.UpdatedAt = DateTime.UtcNow;
        
        _context.Parents.Update(parent);
        return parent;
    }

    public async Task<Parent> UpdateStatusAsync(Guid id, ProfileStatus status)
    {
        var parent = await GetByIdAsync(id);
        
        if (parent == null)
        {
            throw new ParentNotFoundException(id);
        }

        // Validate status transition
        if (parent.Status == ProfileStatus.Suspended && status == ProfileStatus.Active)
        {
            // Allow reactivation
        }
        else if (parent.Status == ProfileStatus.Active && status == ProfileStatus.Suspended)
        {
            // Allow suspension
        }
        else if (parent.Status == ProfileStatus.Pending && status == ProfileStatus.Active)
        {
            // Allow activation
        }
        else if (parent.Status == ProfileStatus.Pending && status == ProfileStatus.Rejected)
        {
            // Allow rejection
        }
        else if (parent.Status == status)
        {
            // No change needed
            return parent;
        }
        else
        {
            throw new InvalidParentStateException(id, parent.Status.ToString(), status.ToString());
        }

        parent.Status = status;
        parent.UpdatedAt = DateTime.UtcNow;
        
        _context.Parents.Update(parent);
        return parent;
    }

    public async Task<Parent> AddChildAsync(Guid parentId, Guid studentId)
    {
        var parent = await GetByIdAsync(parentId);
        
        if (parent == null)
        {
            throw new ParentNotFoundException(parentId);
        }

        // Check if student exists
        var student = await _context.Students
            .Include(s => s.User)
            .Include(s => s.Grade)
            .Include(s => s.Major)
            .FirstOrDefaultAsync(s => s.Id == studentId);
        
        if (student == null)
        {
            throw new ParentBusinessRuleViolationException("StudentValidation", 
                $"Student with ID {studentId} does not exist");
        }

        // Check if student is already a child of this parent
        if (await IsChildOfParentAsync(parentId, studentId))
        {
            throw new DuplicateChildException(parentId, studentId);
        }

        // Check if student already has maximum number of parents (typically 2)
        var parentCount = await GetParentCountForStudentAsync(studentId);
        if (parentCount >= 2)
        {
            throw new MaxParentsExceededException(studentId, 2);
        }

        // Add the child
        parent.Children.Add(student);
        parent.UpdatedAt = DateTime.UtcNow;
        
        _context.Parents.Update(parent);
        return parent;
    }

    public async Task<Parent> RemoveChildAsync(Guid parentId, Guid studentId)
    {
        var parent = await GetByIdAsync(parentId);
        
        if (parent == null)
        {
            throw new ParentNotFoundException(parentId);
        }

        // Check if student is actually a child of this parent
        if (!await IsChildOfParentAsync(parentId, studentId))
        {
            throw new ChildNotAssociatedException(parentId, studentId);
        }

        // Remove the child
        var childToRemove = parent.Children.FirstOrDefault(c => c.Id == studentId);
        if (childToRemove != null)
        {
            parent.Children.Remove(childToRemove);
            parent.UpdatedAt = DateTime.UtcNow;
        }
        
        _context.Parents.Update(parent);
        return parent;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var parent = await GetByIdAsync(id);
        
        if (parent == null)
        {
            return false;
        }

        // Soft delete by setting status to inactive
        parent.Status = ProfileStatus.Inactive;
        parent.UpdatedAt = DateTime.UtcNow;
        
        _context.Parents.Update(parent);
        return true;
    }
}