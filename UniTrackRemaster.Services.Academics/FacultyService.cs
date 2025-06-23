using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Department;
using UniTrackRemaster.Api.Dto.Faculty;
using UniTrackRemaster.Api.Dto.Major;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Enums;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Users;

using Microsoft.AspNetCore.Identity;
using UniTrackRemaster.Commons.Services;

namespace UniTrackRemaster.Services.Academics;



public class FacultyService : IFacultyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FacultyService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public FacultyService(
        IUnitOfWork unitOfWork,
        ILogger<FacultyService> logger,
        UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _userManager = userManager;
    }

    #region Basic CRUD operations

    public async Task<FacultyResponseDto> GetByIdAsync(Guid id)
    {
        var faculty = await _unitOfWork.Faculties.GetByIdWithRelationsAsync(id)
            ?? throw new NotFoundException("Faculty not found");
        return FacultyResponseDto.FromEntity(faculty);
    }

    public async Task<FacultyResponseDto> CreateAsync(CreateFacultyDto dto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Validate university
            var university = await _unitOfWork.Universities.GetByIdAsync(dto.UniversityId);
            if (university == null)
                throw new NotFoundException("University not found");

            // Check if a faculty with the same name already exists for this university
            var existingFacultyByName = await _unitOfWork.Faculties.GetByNameAndUniversityAsync(dto.Name, university.Id);
            if (existingFacultyByName != null)
                throw new ValidationException($"A faculty with the name '{dto.Name}' already exists for this university.");

            // Check if a faculty with the same code already exists for this university
            if (!string.IsNullOrEmpty(dto.Code))
            {
                var existingFacultyByCode = await _unitOfWork.Faculties.GetByCodeAndUniversityAsync(dto.Code, university.Id);
                if (existingFacultyByCode != null)
                    throw new ValidationException($"A faculty with the code '{dto.Code}' already exists for this university.");
            }

            var faculty = new Faculty
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Code = dto.Code,
                ShortDescription = dto.ShortDescription,
                DetailedDescription = dto.DetailedDescription,
                Website = dto.Website,
                ContactEmail = dto.ContactEmail,
                ContactPhone = dto.ContactPhone,
                Status = dto.Status,
                UniversityId = university.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Departments = new List<Department>(),
                Majors = new List<Major>()
            };

            await _unitOfWork.Faculties.CreateAsync(faculty);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Created faculty with ID: {FacultyId}", faculty.Id);
            return FacultyResponseDto.FromEntity(faculty);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error creating faculty: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<FacultyResponseDto> UpdateAsync(Guid id, UpdateFacultyDto dto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var faculty = await _unitOfWork.Faculties.GetByIdWithRelationsAsync(id)
                ?? throw new NotFoundException("Faculty not found");

            // Check for name uniqueness if name is being changed
            if (dto.Name != null && dto.Name != faculty.Name)
            {
                var existingFacultyByName = await _unitOfWork.Faculties.GetByNameAndUniversityAsync(dto.Name, faculty.UniversityId);
                if (existingFacultyByName != null && existingFacultyByName.Id != id)
                    throw new ValidationException($"A faculty with the name '{dto.Name}' already exists for this university.");
            }

            // Check for code uniqueness if code is being changed
            if (dto.Code != null && dto.Code != faculty.Code)
            {
                var existingFacultyByCode = await _unitOfWork.Faculties.GetByCodeAndUniversityAsync(dto.Code, faculty.UniversityId);
                if (existingFacultyByCode != null && existingFacultyByCode.Id != id)
                    throw new ValidationException($"A faculty with the code '{dto.Code}' already exists for this university.");
            }

            // Update basic properties
            if (dto.Name != null) faculty.Name = dto.Name;
            if (dto.Code != null) faculty.Code = dto.Code;
            if (dto.ShortDescription != null) faculty.ShortDescription = dto.ShortDescription;
            if (dto.DetailedDescription != null) faculty.DetailedDescription = dto.DetailedDescription;
            if (dto.Website != null) faculty.Website = dto.Website;
            if (dto.ContactEmail != null) faculty.ContactEmail = dto.ContactEmail;
            if (dto.ContactPhone != null) faculty.ContactPhone = dto.ContactPhone;
            if (dto.Status.HasValue) faculty.Status = dto.Status.Value;

            faculty.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Faculties.UpdateAsync(faculty);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Updated faculty with ID: {FacultyId}", faculty.Id);
            return FacultyResponseDto.FromEntity(faculty);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error updating faculty: {Message}", ex.Message);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var faculty = await _unitOfWork.Faculties.GetByIdWithRelationsAsync(id)
                ?? throw new NotFoundException("Faculty not found");

            bool hasDepartments = await _unitOfWork.Faculties.HasDepartmentsAsync(id);
            if (hasDepartments)
                throw new ValidationException("Cannot delete faculty with associated departments");

            bool hasMajors = await _unitOfWork.Faculties.HasMajorsAsync(id);
            if (hasMajors)
                throw new ValidationException("Cannot delete faculty with associated majors");

            await _unitOfWork.Faculties.DeleteAsync(id);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Deleted faculty with ID: {FacultyId}", id);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error deleting faculty: {Message}", ex.Message);
            throw;
        }
    }

    #endregion

    #region Basic count methods

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Faculties.CountAsync(cancellationToken);
    }

    #endregion

    #region Non-paginated methods (for calculations and internal operations)

    public async Task<IEnumerable<FacultyResponseDto>> GetAllAsync()
    {
        var faculties = await _unitOfWork.Faculties.GetAllWithRelationsAsync();
        return faculties.Select(FacultyResponseDto.FromEntity);
    }

    public async Task<IEnumerable<FacultyResponseDto>> GetByUniversityAsync(Guid universityId)
    {
        var faculties = await _unitOfWork.Faculties.GetByUniversityWithRelationsAsync(universityId);
        return faculties.Select(FacultyResponseDto.FromEntity);
    }

    public async Task<IEnumerable<FacultyResponseDto>> GetByInstitutionIdAsync(Guid institutionId)
    {
        var faculties = await _unitOfWork.Faculties.GetByInstitutionWithRelationsAsync(institutionId);
        return faculties.Select(FacultyResponseDto.FromEntity);
    }

    public async Task<IEnumerable<FacultyResponseDto>> SearchFacultiesAsync(string searchTerm)
    {
        var faculties = await _unitOfWork.Faculties.SearchWithRelationsAsync(searchTerm);
        return faculties.Select(FacultyResponseDto.FromEntity);
    }

    #endregion

    #region Paginated methods with filtering (for API endpoints)

    public async Task<PagedResult<FacultyResponseDto>> GetAllAsync(
        string? query = null,
        string? universityId = null,
        string? institutionId = null,
        string? status = null,
        int page = 1, 
        int pageSize = 50)
    {
        _logger.LogInformation("Retrieving all faculties with filters - Query: {Query}, UniversityId: {UniversityId}, Page: {Page}, PageSize: {PageSize}", 
            query, universityId, page, pageSize);
        
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
            var faculties = await _unitOfWork.Faculties.GetAllWithRelationsAsync(
                query, universityId, institutionId, status, page, pageSize);
            var totalCount = await _unitOfWork.Faculties.GetTotalCountAsync(
                query, universityId, institutionId, status);
            
            var facultyDtos = faculties
                .Select(f => FacultyResponseDto.FromEntity(f))
                .ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            _logger.LogDebug("Successfully retrieved {Count} faculties (Page {Page} of {TotalPages})", 
                facultyDtos.Count, page, totalPages);
            
            return new PagedResult<FacultyResponseDto>
            {
                Items = facultyDtos,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
            };
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex, "Unexpected error retrieving all faculties");
            throw;
        }
    }

    public async Task<PagedResult<FacultyResponseDto>> GetFacultiesByInstitutionAsync(
        Guid institutionId,
        string? query = null,
        string? universityId = null,
        string? status = null,
        int page = 1, 
        int pageSize = 50)
    {
        _logger.LogInformation("Retrieving faculties for institution: {InstitutionId} with filters - Query: {Query}, UniversityId: {UniversityId}, Page: {Page}, PageSize: {PageSize}", 
            institutionId, query, universityId, page, pageSize);
        
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
            var faculties = await _unitOfWork.Faculties.GetFacultiesByInstitutionAsync(
                institutionId, query, universityId, status, page, pageSize);
            var totalCount = await _unitOfWork.Faculties.GetFacultiesByInstitutionCountAsync(
                institutionId, query, universityId, status);
            
            var facultyDtos = faculties
                .Select(f => FacultyResponseDto.FromEntity(f))
                .ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            _logger.LogDebug("Successfully retrieved {Count} faculties for institution {InstitutionId} (Page {Page} of {TotalPages})", 
                facultyDtos.Count, institutionId, page, totalPages);
            
            return new PagedResult<FacultyResponseDto>
            {
                Items = facultyDtos,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
            };
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex, "Unexpected error retrieving faculties for institution {InstitutionId}", institutionId);
            throw;
        }
    }

    #endregion

    #region Faculty-specific functionality

    public async Task<IEnumerable<DepartmentResponseDto>> GetFacultyDepartmentsAsync(Guid facultyId)
    {
        try
        {
            var departmentsData = await _unitOfWork.Faculties.GetFacultyDepartmentsWithValidationAsync(facultyId);

            if (departmentsData.FacultyNotFound)
                throw new NotFoundException("Faculty not found");

            return departmentsData.Departments.Select(DepartmentResponseDto.FromEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving departments for faculty {FacultyId}: {Message}",
                facultyId, ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<MajorResponseDto>> GetFacultyMajorsAsync(Guid facultyId)
    {
        try
        {
            var majorsData = await _unitOfWork.Faculties.GetFacultyMajorsWithValidationAsync(facultyId);

            if (majorsData.FacultyNotFound)
                throw new NotFoundException("Faculty not found");

            return majorsData.Majors.Select(MajorResponseDto.FromEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving majors for faculty {FacultyId}: {Message}",
                facultyId, ex.Message);
            throw;
        }
    }

    #endregion

    #region Access control

    public async Task<bool> CanAccessFaculty(Guid facultyId, Guid userId, Guid institutionId, Roles role)
    {
        if (role is Roles.SuperAdmin)
            return true;
        
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user.IsLinked && user.Institutions.Any((i) => i.Id == institutionId);
    }

    #endregion
}