using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Department;
using UniTrackRemaster.Api.Dto.Teacher;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Enums;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Services.Academics;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DepartmentService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public DepartmentService(
        IUnitOfWork unitOfWork,
        ILogger<DepartmentService> logger,
        UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _userManager = userManager;
    }

    #region Basic CRUD operations

    public async Task<DepartmentResponseDto> GetByIdAsync(Guid id)
    {
        var department = await _unitOfWork.Departments.GetByIdWithRelationsAsync(id)
            ?? throw new NotFoundException("Department not found");
        return DepartmentResponseDto.FromEntity(department);
    }

    public async Task<DepartmentResponseDto> CreateAsync(CreateDepartmentDto dto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Validate faculty
            var faculty = await _unitOfWork.Faculties.GetByIdAsync(dto.FacultyId);
            if (faculty == null)
                throw new NotFoundException("Faculty not found");

            // Check if a department with the same name already exists for this faculty
            var existingDepartmentByName = await _unitOfWork.Departments.GetByNameAndFacultyAsync(dto.Name, faculty.Id);
            if (existingDepartmentByName != null)
                throw new ValidationException($"A department with the name '{dto.Name}' already exists for this faculty.");

            // Check if a department with the same code already exists for this faculty
            if (!string.IsNullOrEmpty(dto.Code))
            {
                var existingDepartmentByCode = await _unitOfWork.Departments.GetByCodeAndFacultyAsync(dto.Code, faculty.Id);
                if (existingDepartmentByCode != null)
                    throw new ValidationException($"A department with the code '{dto.Code}' already exists for this faculty.");
            }

            var department = new Department
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Code = dto.Code,
                Description = dto.Description,
                Location = dto.Location,
                ContactEmail = dto.ContactEmail,
                ContactPhone = dto.ContactPhone,
                Type = dto.Type,
                Status = dto.Status,
                FacultyId = faculty.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Teachers = new List<Teacher>()
            };

            await _unitOfWork.Departments.CreateAsync(department);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Created department with ID: {DepartmentId}", department.Id);
            return DepartmentResponseDto.FromEntity(department);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error creating department: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<DepartmentResponseDto> UpdateAsync(Guid id, UpdateDepartmentDto dto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var department = await _unitOfWork.Departments.GetByIdWithRelationsAsync(id)
                ?? throw new NotFoundException("Department not found");

            // Check for name uniqueness if name is being changed
            if (dto.Name != null && dto.Name != department.Name)
            {
                var existingDepartmentByName = await _unitOfWork.Departments.GetByNameAndFacultyAsync(dto.Name, department.FacultyId);
                if (existingDepartmentByName != null && existingDepartmentByName.Id != id)
                    throw new ValidationException($"A department with the name '{dto.Name}' already exists for this faculty.");
            }

            // Check for code uniqueness if code is being changed
            if (dto.Code != null && dto.Code != department.Code)
            {
                var existingDepartmentByCode = await _unitOfWork.Departments.GetByCodeAndFacultyAsync(dto.Code, department.FacultyId);
                if (existingDepartmentByCode != null && existingDepartmentByCode.Id != id)
                    throw new ValidationException($"A department with the code '{dto.Code}' already exists for this faculty.");
            }

            // Update basic properties
            if (dto.Name != null) department.Name = dto.Name;
            if (dto.Code != null) department.Code = dto.Code;
            if (dto.Description != null) department.Description = dto.Description;
            if (dto.Location != null) department.Location = dto.Location;
            if (dto.ContactEmail != null) department.ContactEmail = dto.ContactEmail;
            if (dto.ContactPhone != null) department.ContactPhone = dto.ContactPhone;
            if (dto.Type.HasValue) department.Type = dto.Type.Value;
            if (dto.Status.HasValue) department.Status = dto.Status.Value;

            department.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Departments.UpdateAsync(department);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Updated department with ID: {DepartmentId}", department.Id);
            return DepartmentResponseDto.FromEntity(department);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error updating department: {Message}", ex.Message);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var department = await _unitOfWork.Departments.GetByIdWithRelationsAsync(id)
                ?? throw new NotFoundException("Department not found");

            bool hasTeachers = await _unitOfWork.Departments.HasTeachersAsync(id);
            if (hasTeachers)
                throw new ValidationException("Cannot delete department with assigned teachers");

            await _unitOfWork.Departments.DeleteAsync(id);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Deleted department with ID: {DepartmentId}", id);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error deleting department: {Message}", ex.Message);
            throw;
        }
    }

    #endregion

    #region Basic count methods

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Departments.CountAsync(cancellationToken);
    }

    #endregion

    #region Non-paginated methods (for calculations and internal operations)

    public async Task<IEnumerable<DepartmentResponseDto>> GetAllAsync()
    {
        var departments = await _unitOfWork.Departments.GetAllWithRelationsAsync();
        return departments.Select(DepartmentResponseDto.FromEntity);
    }

    public async Task<IEnumerable<DepartmentResponseDto>> GetByFacultyAsync(Guid facultyId)
    {
        var departments = await _unitOfWork.Departments.GetByFacultyWithRelationsAsync(facultyId);
        return departments.Select(DepartmentResponseDto.FromEntity);
    }

    public async Task<IEnumerable<DepartmentResponseDto>> GetByInstitutionIdAsync(Guid institutionId)
    {
        var departments = await _unitOfWork.Departments.GetByInstitutionWithRelationsAsync(institutionId);
        return departments.Select(DepartmentResponseDto.FromEntity);
    }

    public async Task<IEnumerable<DepartmentResponseDto>> SearchDepartmentsAsync(string searchTerm)
    {
        var departments = await _unitOfWork.Departments.SearchWithRelationsAsync(searchTerm);
        return departments.Select(DepartmentResponseDto.FromEntity);
    }

    #endregion

    #region Paginated methods with filtering (for API endpoints)

    public async Task<PagedResult<DepartmentResponseDto>> GetAllAsync(
        string? query = null,
        string? facultyId = null,
        string? institutionId = null,
        string? type = null,
        string? status = null,
        int page = 1, 
        int pageSize = 50)
    {
        _logger.LogInformation("Retrieving all departments with filters - Query: {Query}, FacultyId: {FacultyId}, Page: {Page}, PageSize: {PageSize}", 
            query, facultyId, page, pageSize);
        
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
            var departments = await _unitOfWork.Departments.GetAllWithRelationsAsync(
                query, facultyId, institutionId, type, status, page, pageSize);
            var totalCount = await _unitOfWork.Departments.GetTotalCountAsync(
                query, facultyId, institutionId, type, status);
            
            var departmentDtos = departments
                .Select(d => DepartmentResponseDto.FromEntity(d))
                .ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            _logger.LogDebug("Successfully retrieved {Count} departments (Page {Page} of {TotalPages})", 
                departmentDtos.Count, page, totalPages);
            
            return new PagedResult<DepartmentResponseDto>
            {
                Items = departmentDtos,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
            };
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex, "Unexpected error retrieving all departments");
            throw;
        }
    }

    public async Task<PagedResult<DepartmentResponseDto>> GetDepartmentsByInstitutionAsync(
        Guid institutionId,
        string? query = null,
        string? facultyId = null,
        string? type = null,
        string? status = null,
        int page = 1, 
        int pageSize = 50)
    {
        _logger.LogInformation("Retrieving departments for institution: {InstitutionId} with filters - Query: {Query}, FacultyId: {FacultyId}, Page: {Page}, PageSize: {PageSize}", 
            institutionId, query, facultyId, page, pageSize);
        
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
            var departments = await _unitOfWork.Departments.GetDepartmentsByInstitutionAsync(
                institutionId, query, facultyId, type, status, page, pageSize);
            var totalCount = await _unitOfWork.Departments.GetDepartmentsByInstitutionCountAsync(
                institutionId, query, facultyId, type, status);
            
            var departmentDtos = departments
                .Select(d => DepartmentResponseDto.FromEntity(d))
                .ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            _logger.LogDebug("Successfully retrieved {Count} departments for institution {InstitutionId} (Page {Page} of {TotalPages})", 
                departmentDtos.Count, institutionId, page, totalPages);
            
            return new PagedResult<DepartmentResponseDto>
            {
                Items = departmentDtos,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
            };
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex, "Unexpected error retrieving departments for institution {InstitutionId}", institutionId);
            throw;
        }
    }

    #endregion

    #region Department-specific functionality

    public async Task<IEnumerable<TeacherResponseDto>> GetDepartmentTeachersAsync(Guid departmentId)
    {
        try
        {
            var teachersData = await _unitOfWork.Departments.GetDepartmentTeachersWithValidationAsync(departmentId);

            if (teachersData.DepartmentNotFound)
                throw new NotFoundException("Department not found");

            return teachersData.Teachers.Select(t => TeacherResponseDto.FromEntity(t, t.User));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving teachers for department {DepartmentId}: {Message}",
                departmentId, ex.Message);
            throw;
        }
    }

    public async Task AssignTeacherAsync(Guid departmentId, Guid teacherId)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var validation = await _unitOfWork.Departments.ValidateTeacherAssignmentAsync(departmentId, teacherId);
            

            if (validation.TeacherNotFound)
                throw new NotFoundException("Teacher not found");

            if (validation.AlreadyAssigned)
                throw new ValidationException("Teacher is already assigned to this department");

            await _unitOfWork.Departments.AssignTeacherAsync(departmentId, teacherId);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Assigned teacher {TeacherId} to department {DepartmentId}", teacherId, departmentId);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error assigning teacher to department: {Message}", ex.Message);
            throw;
        }
    }

    public async Task RemoveTeacherAsync(Guid departmentId, Guid teacherId)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var removed = await _unitOfWork.Departments.RemoveTeacherAsync(departmentId, teacherId);

            if (!removed)
                throw new NotFoundException("Teacher assignment not found");

            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Removed teacher {TeacherId} from department {DepartmentId}", teacherId, departmentId);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error removing teacher from department: {Message}", ex.Message);
            throw;
        }
    }

    #endregion

    #region Access control

    public async Task<bool> CanAccessDepartment(Guid departmentId, Guid userId, Guid institutionId, Roles role)
    {
        if (role is Roles.SuperAdmin)
            return true;
        
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user.IsLinked && user.Institutions.Any((i) => i.Id == institutionId);
    }

    #endregion
}