using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Student;
using UniTrackRemaster.Api.Dto.Subject;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Enums;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Services.Academics;

public class SubjectService : ISubjectService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SubjectService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public SubjectService(
        IUnitOfWork unitOfWork,
        ILogger<SubjectService> logger,
        UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _userManager = userManager;
    }

    #region Basic CRUD operations

    public async Task<SubjectResponseDto> GetByIdAsync(Guid id)
    {
        var subject = await _unitOfWork.Subjects.GetByIdWithRelationsAsync(id)
            ?? throw new NotFoundException("Subject not found");
        return SubjectResponseDto.FromEntity(subject);
    }

    public async Task<SubjectResponseDto> CreateAsync(CreateSubjectDto dto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var subject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Code = dto.Code,
                ShortDescription = dto.ShortDescription,
                DetailedDescription = dto.DetailedDescription,
                SubjectType = dto.SubjectType,
                AcademicLevel = dto.AcademicLevel,
                MinGradeLevel = dto.MinGradeLevel,
                MaxGradeLevel = dto.MaxGradeLevel,
                CreditHours = dto.CreditHours,
                CreditValue = dto.CreditValue,
                IsElective = dto.IsElective,
                ElectiveType = dto.ElectiveType,
                MaxStudents = dto.MaxStudents,
                HasLab = dto.HasLab,
                PrimaryTeacherId = dto.PrimaryTeacherId,
                DepartmentId = dto.DepartmentId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Grades = new List<Grade>(),
                Teachers = new List<Teacher>(),
                StudentElectives = new List<StudentElective>(),
                InstitutionId = dto.InstitutionId
            };

            if (dto.TeacherIds?.Any() == true)
            {
                var teachers = await _unitOfWork.Teachers.GetByIdsAsync(dto.TeacherIds);
                foreach (var teacher in teachers)
                {
                    subject.Teachers.Add(teacher);
                }
            }

            if (dto.GradeIds?.Any() == true)
            {
                var grades = await _unitOfWork.Grades.GetByIdsAsync(dto.GradeIds);
                foreach (var grade in grades)
                {
                    subject.Grades.Add(grade);
                }
            }

            await _unitOfWork.Subjects.CreateAsync(subject);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Created subject with ID: {SubjectId}", subject.Id);
            return SubjectResponseDto.FromEntity(subject);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error creating subject: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<SubjectResponseDto> UpdateAsync(Guid id, UpdateSubjectDto dto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var subject = await _unitOfWork.Subjects.GetByIdWithRelationsAsync(id)
                ?? throw new NotFoundException("Subject not found");

            // Update basic properties
            if (dto.Name != null) subject.Name = dto.Name;
            if (dto.Code != null) subject.Code = dto.Code;
            if (dto.ShortDescription != null) subject.ShortDescription = dto.ShortDescription;
            if (dto.DetailedDescription != null) subject.DetailedDescription = dto.DetailedDescription;
            if (dto.SubjectType.HasValue) subject.SubjectType = dto.SubjectType.Value;
            if (dto.AcademicLevel.HasValue) subject.AcademicLevel = dto.AcademicLevel;
            if (dto.MinGradeLevel.HasValue) subject.MinGradeLevel = dto.MinGradeLevel;
            if (dto.MaxGradeLevel.HasValue) subject.MaxGradeLevel = dto.MaxGradeLevel;
            if (dto.CreditHours.HasValue) subject.CreditHours = dto.CreditHours;
            if (dto.CreditValue.HasValue) subject.CreditValue = dto.CreditValue;
            if (dto.IsElective.HasValue) subject.IsElective = dto.IsElective.Value;
            if (dto.ElectiveType.HasValue) subject.ElectiveType = dto.ElectiveType;
            if (dto.MaxStudents.HasValue) subject.MaxStudents = dto.MaxStudents;
            if (dto.HasLab.HasValue) subject.HasLab = dto.HasLab.Value;
            if (dto.PrimaryTeacherId.HasValue) subject.PrimaryTeacherId = dto.PrimaryTeacherId;
            if (dto.DepartmentId.HasValue) subject.DepartmentId = dto.DepartmentId;

            // Update relationships
            if (dto.TeacherIds != null)
            {
                await _unitOfWork.Subjects.UpdateTeachersAsync(id, dto.TeacherIds);
            }

            if (dto.GradeIds != null)
            {
                await _unitOfWork.Subjects.UpdateGradesAsync(id, dto.GradeIds);
            }

            subject.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Subjects.UpdateAsync(subject);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Updated subject with ID: {SubjectId}", subject.Id);
            return SubjectResponseDto.FromEntity(subject);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error updating subject: {Message}", ex.Message);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            bool hasStudents = await _unitOfWork.Subjects.HasEnrolledStudentsAsync(id);
            if (hasStudents)
                throw new ValidationException("Cannot delete subject with enrolled students");

            await _unitOfWork.Subjects.DeleteAsync(id);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Deleted subject with ID: {SubjectId}", id);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error deleting subject: {Message}", ex.Message);
            throw;
        }
    }

    #endregion

    #region Basic count methods

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Subjects.CountAsync(cancellationToken);
    }

    #endregion

    #region Non-paginated methods (for calculations and internal operations)

    public async Task<IEnumerable<SubjectResponseDto>> GetAllAsync()
    {
        var subjects = await _unitOfWork.Subjects.GetAllWithRelationsAsync();
        return subjects.Select(SubjectResponseDto.FromEntity);
    }

    public async Task<IEnumerable<SubjectResponseDto>> GetByDepartmentAsync(Guid departmentId)
    {
        var subjects = await _unitOfWork.Subjects.GetByDepartmentWithRelationsAsync(departmentId);
        return subjects.Select(SubjectResponseDto.FromEntity);
    }

    public async Task<IEnumerable<SubjectResponseDto>> GetElectivesAsync(bool activeOnly = true)
    {
        var subjects = await _unitOfWork.Subjects.GetElectivesWithRelationsAsync(activeOnly);
        return subjects.Select(SubjectResponseDto.FromEntity);
    }

    public async Task<IEnumerable<SubjectResponseDto>> SearchSubjectsAsync(string searchTerm)
    {
        var subjects = await _unitOfWork.Subjects.SearchWithRelationsAsync(searchTerm);
        return subjects.Select(SubjectResponseDto.FromEntity);
    }

    #endregion

    #region Paginated methods with filtering (for API endpoints)

    public async Task<PagedResult<SubjectResponseDto>> GetAllAsync(
        string? query = null,
        string? departmentId = null,
        string? academicLevel = null,
        string? electiveType = null,
        bool? hasLab = null,
        bool? isElective = null,
        int page = 1, 
        int pageSize = 50)
    {
        _logger.LogInformation("Retrieving all subjects with filters - Query: {Query}, DepartmentId: {DepartmentId}, Page: {Page}, PageSize: {PageSize}", 
            query, departmentId, page, pageSize);
        
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
            var subjects = await _unitOfWork.Subjects.GetAllWithRelationsAsync(
                query, departmentId, academicLevel, electiveType, hasLab, isElective, page, pageSize);
            var totalCount = await _unitOfWork.Subjects.GetTotalCountAsync(
                query, departmentId, academicLevel, electiveType, hasLab, isElective);
            
            var subjectDtos = subjects
                .Select(s => SubjectResponseDto.FromEntity(s))
                .ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            _logger.LogDebug("Successfully retrieved {Count} subjects (Page {Page} of {TotalPages})", 
                subjectDtos.Count, page, totalPages);
            
            return new PagedResult<SubjectResponseDto>
            {
                Items = subjectDtos,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
            };
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex, "Unexpected error retrieving all subjects");
            throw;
        }
    }

    public async Task<PagedResult<SubjectResponseDto>> GetSubjectsByInstitutionAsync(
        Guid institutionId,
        string? query = null,
        string? departmentId = null,
        string? academicLevel = null,
        string? electiveType = null,
        bool? hasLab = null,
        bool? isElective = null,
        int page = 1, 
        int pageSize = 50)
    {
        _logger.LogInformation("Retrieving subjects for institution: {InstitutionId} with filters - Query: {Query}, DepartmentId: {DepartmentId}, Page: {Page}, PageSize: {PageSize}", 
            institutionId, query, departmentId, page, pageSize);
        
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
            var subjects = await _unitOfWork.Subjects.GetSubjectsByInstitutionAsync(
                institutionId, query, departmentId, academicLevel, electiveType, hasLab, isElective, page, pageSize);
            var totalCount = await _unitOfWork.Subjects.GetSubjectsByInstitutionCountAsync(
                institutionId, query, departmentId, academicLevel, electiveType, hasLab, isElective);
            
            var subjectDtos = subjects
                .Select(s => SubjectResponseDto.FromEntity(s))
                .ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            _logger.LogDebug("Successfully retrieved {Count} subjects for institution {InstitutionId} (Page {Page} of {TotalPages})", 
                subjectDtos.Count, institutionId, page, totalPages);
            
            return new PagedResult<SubjectResponseDto>
            {
                Items = subjectDtos,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
            };
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex, "Unexpected error retrieving subjects for institution {InstitutionId}", institutionId);
            throw;
        }
    }

    #endregion

    #region Elective management

    public async Task EnrollStudentInElectiveAsync(Guid subjectId, Guid studentId)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var enrollmentValidation = await _unitOfWork.Subjects.ValidateElectiveEnrollmentAsync(subjectId, studentId);

            if (!enrollmentValidation.IsElective)
                throw new ValidationException("Subject is not an elective");

            if (enrollmentValidation.StudentNotFound)
                throw new NotFoundException("Student not found");

            if (enrollmentValidation.AlreadyEnrolled)
                throw new ValidationException("Student is already enrolled in this elective");

            if (enrollmentValidation.MaxCapacityReached)
                throw new ValidationException("Elective has reached maximum enrollment capacity");

            var enrollment = new StudentElective
            {
                Id = Guid.NewGuid(),
                StudentId = studentId,
                SubjectId = subjectId,
                EnrollmentDate = DateTime.UtcNow,
                Status = ElectiveStatus.Enrolled
            };

            await _unitOfWork.Subjects.AddStudentElectiveAsync(enrollment);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Enrolled student {StudentId} in elective {SubjectId}", studentId, subjectId);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error enrolling student in elective: {Message}", ex.Message);
            throw;
        }
    }

    public async Task UnenrollStudentFromElectiveAsync(Guid subjectId, Guid studentId)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var updated = await _unitOfWork.Subjects.UpdateStudentElectiveStatusAsync(
                subjectId, studentId, ElectiveStatus.Dropped);

            if (!updated)
                throw new NotFoundException("Student is not enrolled in this elective");

            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Unenrolled student {StudentId} from elective {SubjectId}", studentId, subjectId);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error unenrolling student from elective: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<StudentElectiveResponseDto>> GetElectiveEnrollmentsAsync(Guid subjectId)
    {
        try
        {
            var enrollmentData = await _unitOfWork.Subjects.GetElectiveEnrollmentsWithDetailsAsync(subjectId);

            if (!enrollmentData.IsElective)
                throw new ValidationException("Subject is not an elective");

            if (enrollmentData.SubjectNotFound)
                throw new NotFoundException("Subject not found");

            return enrollmentData.Enrollments.Select(StudentElectiveResponseDto.FromEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving enrollments for elective {SubjectId}: {Message}",
                subjectId, ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<SubjectResponseDto>> GetStudentElectivesAsync(Guid studentId)
    {
        try
        {
            var electivesData = await _unitOfWork.Subjects.GetStudentElectivesWithDetailsAsync(studentId);

            if (electivesData.StudentNotFound)
                throw new NotFoundException("Student not found");

            return electivesData.Subjects.Select(SubjectResponseDto.FromEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving electives for student {StudentId}: {Message}",
                studentId, ex.Message);
            throw;
        }
    }

    #endregion

    #region Teacher and student access methods

    public async Task<IEnumerable<StudentResponseDto>> GetStudentsBySubjectAsync(Guid subjectId, Guid teacherId)
    {
        try
        {
            var studentsData = await _unitOfWork.Subjects.GetStudentsBySubjectWithTeacherValidationAsync(subjectId, teacherId);

            if (!studentsData.TeacherTeachesSubject)
                throw new ValidationException("Teacher does not teach this subject");

            return studentsData.Students.Select(StudentResponseDto.FromEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving students for subject {SubjectId} and teacher {TeacherId}: {Message}",
                subjectId, teacherId, ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<SubjectResponseDto>> GetSubjectsByTeacherAsync(Guid teacherId)
    {
        try
        {
            var subjectsData = await _unitOfWork.Subjects.GetAllSubjectsByTeacherAsync(teacherId);

            if (subjectsData.TeacherNotFound)
                throw new NotFoundException("Teacher not found");

            return subjectsData.Subjects.Select(SubjectResponseDto.FromEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subjects for teacher {TeacherId}: {Message}",
                teacherId, ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<StudentsByGradeDto>> GetStudentsBySubjectAndGradeAsync(Guid subjectId, Guid teacherId)
    {
        try
        {
            var data = await _unitOfWork.Subjects.GetStudentsBySubjectAndGradeWithTeacherValidationAsync(subjectId, teacherId);

            if (!data.TeacherTeachesSubject)
                throw new ValidationException("Teacher does not teach this subject");

            if (data.SubjectNotFound)
                throw new NotFoundException("Subject not found");

            return data.StudentsByGrade.Select(g => new StudentsByGradeDto(
                g.GradeId,
                g.GradeName,
                g.Students.Select(StudentResponseDto.FromEntity).ToList()
            )).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving students by grade for subject {SubjectId} and teacher {TeacherId}: {Message}",
                subjectId, teacherId, ex.Message);
            throw;
        }
    }

    #endregion

    #region Access control

    public async Task<bool> CanAccessSubject(Guid subjectId, Guid userId, Guid institutionId, Roles role)
    {
        if (role is Roles.SuperAdmin)
            return true;
        
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user.IsLinked && user.Institutions.Any((i) => i.Id == institutionId);
    }

    #endregion
}