using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Grade;
using UniTrackRemaster.Api.Dto.Student;
using UniTrackRemaster.Api.Dto.Teacher;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Enums;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Services.Academics;


public class GradeService : IGradeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GradeService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public GradeService(
        IUnitOfWork unitOfWork,
        ILogger<GradeService> logger,
        UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _userManager = userManager;
    }

    #region Basic CRUD operations

    public async Task<GradeResponseDto> GetByIdAsync(Guid id)
    {
        var grade = await _unitOfWork.Grades.GetByIdWithRelationsAsync(id)
            ?? throw new NotFoundException("Grade not found");
        return GradeResponseDto.FromEntity(grade);
    }

    public async Task<GradeResponseDto> CreateAsync(CreateGradeDto dto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Validate institution
            var institution = await _unitOfWork.Institutions.GetByIdAsync(dto.InstitutionId);
            if (institution == null)
                throw new NotFoundException("Institution not found");

            // Validate academic year
            var academicYear = await _unitOfWork.AcademicYears.GetByIdAsync(dto.AcademicYearId);
            if (academicYear == null)
                throw new NotFoundException("Academic year not found");

            // Validate homeroom teacher if provided
            if (dto.HomeRoomTeacherId.HasValue)
            {
                var teacher = await _unitOfWork.Teachers.GetByIdAsync(dto.HomeRoomTeacherId.Value);
                if (teacher == null)
                    throw new NotFoundException("Homeroom teacher not found");
            }

            var grade = new Grade
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                InstitutionId = dto.InstitutionId,
                AcademicYearId = dto.AcademicYearId,
                HomeRoomTeacherId = dto.HomeRoomTeacherId ?? Guid.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Students = new List<Student>(),
                Teachers = new List<Teacher>()
            };

            await _unitOfWork.Grades.CreateAsync(grade);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Created grade with ID: {GradeId}", grade.Id);
            return GradeResponseDto.FromEntity(grade);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error creating grade: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<GradeResponseDto> UpdateAsync(Guid id, UpdateGradeDto dto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var grade = await _unitOfWork.Grades.GetByIdWithRelationsAsync(id)
                ?? throw new NotFoundException("Grade not found");

            // Update basic properties
            if (dto.Name != null) grade.Name = dto.Name;

            if (dto.InstitutionId.HasValue)
            {
                var institution = await _unitOfWork.Institutions.GetByIdAsync(dto.InstitutionId.Value);
                if (institution == null)
                    throw new NotFoundException("Institution not found");
                grade.InstitutionId = dto.InstitutionId.Value;
            }

            if (dto.AcademicYearId.HasValue)
            {
                var academicYear = await _unitOfWork.AcademicYears.GetByIdAsync(dto.AcademicYearId.Value);
                if (academicYear == null)
                    throw new NotFoundException("Academic year not found");
                grade.AcademicYearId = dto.AcademicYearId.Value;
            }

            if (dto.HomeRoomTeacherId.HasValue)
            {
                if (dto.HomeRoomTeacherId.Value != Guid.Empty)
                {
                    var teacher = await _unitOfWork.Teachers.GetByIdAsync(dto.HomeRoomTeacherId.Value);
                    if (teacher == null)
                        throw new NotFoundException("Homeroom teacher not found");
                }
                grade.HomeRoomTeacherId = dto.HomeRoomTeacherId.Value == Guid.Empty ? Guid.Empty : dto.HomeRoomTeacherId.Value;
            }

            grade.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Grades.UpdateAsync(grade);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Updated grade with ID: {GradeId}", grade.Id);
            return GradeResponseDto.FromEntity(grade);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error updating grade: {Message}", ex.Message);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var grade = await _unitOfWork.Grades.GetByIdWithRelationsAsync(id)
                ?? throw new NotFoundException("Grade not found");

            bool hasStudents = await _unitOfWork.Grades.HasStudentsAsync(id);
            if (hasStudents)
                throw new ValidationException("Cannot delete grade with enrolled students");

            await _unitOfWork.Grades.DeleteAsync(id);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Deleted grade with ID: {GradeId}", id);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error deleting grade: {Message}", ex.Message);
            throw;
        }
    }

    #endregion

    #region Basic count methods

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Grades.CountAsync(cancellationToken);
    }

    #endregion

    #region Non-paginated methods (for calculations and internal operations)

    public async Task<IEnumerable<GradeResponseDto>> GetAllAsync()
    {
        var grades = await _unitOfWork.Grades.GetAllWithRelationsAsync();
        return grades.Select(GradeResponseDto.FromEntity);
    }

    public async Task<IEnumerable<GradeResponseDto>> GetByInstitutionIdAsync(Guid institutionId)
    {
        var grades = await _unitOfWork.Grades.GetByInstitutionWithRelationsAsync(institutionId);
        return grades.Select(GradeResponseDto.FromEntity);
    }

    public async Task<IEnumerable<GradeResponseDto>> GetByAcademicYearIdAsync(Guid academicYearId)
    {
        var grades = await _unitOfWork.Grades.GetByAcademicYearWithRelationsAsync(academicYearId);
        return grades.Select(GradeResponseDto.FromEntity);
    }

    public async Task<IEnumerable<GradeResponseDto>> SearchGradesAsync(string searchTerm)
    {
        var grades = await _unitOfWork.Grades.SearchWithRelationsAsync(searchTerm);
        return grades.Select(GradeResponseDto.FromEntity);
    }

    #endregion

    #region Paginated methods with filtering (for API endpoints)

    public async Task<PagedResult<GradeResponseDto>> GetAllAsync(
        string? query = null,
        string? institutionId = null,
        string? academicYearId = null,
        bool? hasHomeRoomTeacher = null,
        int page = 1, 
        int pageSize = 50)
    {
        _logger.LogInformation("Retrieving all grades with filters - Query: {Query}, InstitutionId: {InstitutionId}, Page: {Page}, PageSize: {PageSize}", 
            query, institutionId, page, pageSize);
        
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
            var grades = await _unitOfWork.Grades.GetAllWithRelationsAsync(
                query, institutionId, academicYearId, hasHomeRoomTeacher, page, pageSize);
            var totalCount = await _unitOfWork.Grades.GetTotalCountAsync(
                query, institutionId, academicYearId, hasHomeRoomTeacher);
            
            var gradeDtos = grades
                .Select(g => GradeResponseDto.FromEntity(g))
                .ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            _logger.LogDebug("Successfully retrieved {Count} grades (Page {Page} of {TotalPages})", 
                gradeDtos.Count, page, totalPages);
            
            return new PagedResult<GradeResponseDto>
            {
                Items = gradeDtos,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
            };
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex, "Unexpected error retrieving all grades");
            throw;
        }
    }

    public async Task<PagedResult<GradeResponseDto>> GetGradesByInstitutionAsync(
        Guid institutionId,
        string? query = null,
        string? academicYearId = null,
        bool? hasHomeRoomTeacher = null,
        int page = 1, 
        int pageSize = 50)
    {
        _logger.LogInformation("Retrieving grades for institution: {InstitutionId} with filters - Query: {Query}, AcademicYearId: {AcademicYearId}, Page: {Page}, PageSize: {PageSize}", 
            institutionId, query, academicYearId, page, pageSize);
        
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
            var grades = await _unitOfWork.Grades.GetGradesByInstitutionAsync(
                institutionId, query, academicYearId, hasHomeRoomTeacher, page, pageSize);
            var totalCount = await _unitOfWork.Grades.GetGradesByInstitutionCountAsync(
                institutionId, query, academicYearId, hasHomeRoomTeacher);
            
            var gradeDtos = grades
                .Select(g => GradeResponseDto.FromEntity(g))
                .ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            _logger.LogDebug("Successfully retrieved {Count} grades for institution {InstitutionId} (Page {Page} of {TotalPages})", 
                gradeDtos.Count, institutionId, page, totalPages);
            
            return new PagedResult<GradeResponseDto>
            {
                Items = gradeDtos,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
            };
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex, "Unexpected error retrieving grades for institution {InstitutionId}", institutionId);
            throw;
        }
    }

    #endregion

    #region Grade-specific functionality

    public async Task<IEnumerable<TeacherResponseDto>> GetAssignedTeachersAsync(Guid gradeId)
    {
        try
        {
            var teachersData = await _unitOfWork.Grades.GetGradeTeachersWithValidationAsync(gradeId);

            if (teachersData.GradeNotFound)
                throw new NotFoundException("Grade not found");

            return teachersData.Teachers.Select( t=> TeacherResponseDto.FromEntity(t, t.User));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving teachers for grade {GradeId}: {Message}",
                gradeId, ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<StudentResponseDto>> GetGradeStudentsAsync(Guid gradeId)
    {
        try
        {
            var studentsData = await _unitOfWork.Grades.GetGradeStudentsWithValidationAsync(gradeId);

            if (studentsData.GradeNotFound)
                throw new NotFoundException("Grade not found");

            return studentsData.Students.Select(StudentResponseDto.FromEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving students for grade {GradeId}: {Message}",
                gradeId, ex.Message);
            throw;
        }
    }

    public async Task AssignTeacherAsync(Guid gradeId, Guid teacherId)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            await _unitOfWork.Grades.AssignTeacherAsync(gradeId, teacherId);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Assigned teacher {TeacherId} to grade {GradeId}", teacherId, gradeId);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error assigning teacher to grade: {Message}", ex.Message);
            throw;
        }
    }

    public async Task RemoveTeacherAsync(Guid gradeId, Guid teacherId)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var removed = await _unitOfWork.Grades.RemoveTeacherAsync(gradeId, teacherId);

            if (!removed)
                throw new NotFoundException("Teacher assignment not found");

            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Removed teacher {TeacherId} from grade {GradeId}", teacherId, gradeId);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error removing teacher from grade: {Message}", ex.Message);
            throw;
        }
    }

    #endregion

    #region Access control

    public async Task<bool> CanAccessGrade(Guid gradeId, Guid userId, Guid institutionId, Roles role)
    {
        if (role is Roles.SuperAdmin)
            return true;
        
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user.IsLinked && user.Institutions.Any((i) => i.Id == institutionId);
    }

    #endregion
}