using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Absence;
using UniTrackRemaster.Api.Dto.Teacher.Analytics;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Services.Academics;


/// <summary>
/// Service for managing absences with semester support.
/// Provides comprehensive absence tracking with semester-aware filtering and analytics.
/// </summary>
public class AbsenceService : IAbsenceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AbsenceService> _logger;

    public AbsenceService(IUnitOfWork unitOfWork, ILogger<AbsenceService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all absences in the system
    /// </summary>
    public async Task<IEnumerable<AbsenceResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var absences = await _unitOfWork.Absences.GetAllAsync(cancellationToken);
        return absences.Select(AbsenceResponseDto.FromEntity);
    }

    /// <summary>
    /// Retrieves a specific absence by ID
    /// </summary>
    public async Task<AbsenceResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var absence = await _unitOfWork.Absences.GetByIdAsync(id, cancellationToken);
        if (absence == null)
            throw new NotFoundException($"Absence with ID {id} not found");

        return AbsenceResponseDto.FromEntity(absence);
    }

    /// <summary>
    /// Retrieves absences by student ID with optional semester filtering
    /// </summary>
    public async Task<IEnumerable<AbsenceResponseDto>> GetByStudentIdAsync(
        Guid studentId, 
        Guid? semesterId = null, 
        CancellationToken cancellationToken = default)
    {
        var absences = await _unitOfWork.Absences.GetByStudentIdAsync(studentId, semesterId, cancellationToken);
        return absences.Select(AbsenceResponseDto.FromEntity);
    }

    /// <summary>
    /// Retrieves absences by teacher ID with optional semester filtering
    /// </summary>
    public async Task<IEnumerable<AbsenceResponseDto>> GetAbsencesByTeacherAsync(
        Guid teacherId, 
        Guid? semesterId = null, 
        CancellationToken cancellationToken = default)
    {
        var absences = await _unitOfWork.Absences.GetAbsencesByTeacherAsync(teacherId, semesterId);
        return absences.Select(AbsenceResponseDto.FromEntity);
    }

    /// <summary>
    /// Retrieves absences by student and subject with optional semester filtering
    /// </summary>
    public async Task<IEnumerable<AbsenceResponseDto>> GetByStudentAndSubjectAsync(
        Guid studentId, 
        Guid subjectId, 
        Guid? semesterId = null, 
        CancellationToken cancellationToken = default)
    {
        var absences = await _unitOfWork.Absences.GetByStudentAndSubjectAsync(studentId, subjectId, semesterId);
        return absences.Select(AbsenceResponseDto.FromEntity);
    }

    /// <summary>
    /// Retrieves absences by subject ID with optional semester filtering
    /// </summary>
    public async Task<IEnumerable<AbsenceResponseDto>> GetBySubjectAsync(
        Guid subjectId, 
        Guid? semesterId = null, 
        CancellationToken cancellationToken = default)
    {
        var absences = await _unitOfWork.Absences.GetBySubjectAsync(subjectId, semesterId);
        return absences.Select(AbsenceResponseDto.FromEntity);
    }

    /// <summary>
    /// Creates a new absence record with automatic semester detection
    /// </summary>
    public async Task<AbsenceResponseDto> CreateAsync(CreateAbsenceDto createAbsenceDto, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Validate existence of related entities
            var student = await _unitOfWork.Students.GetByIdAsync(createAbsenceDto.StudentId);
            if (student == null)
                throw new NotFoundException($"Student with ID {createAbsenceDto.StudentId} not found");

            var teacher = await _unitOfWork.Teachers.GetByIdAsync(createAbsenceDto.TeacherId);
            if (teacher == null)
                throw new NotFoundException($"Teacher with ID {createAbsenceDto.TeacherId} not found");

            if (createAbsenceDto.SubjectId.HasValue)
            {
                var subject = await _unitOfWork.Subjects.GetByIdAsync(createAbsenceDto.SubjectId.Value);
                if (subject == null)
                    throw new NotFoundException($"Subject with ID {createAbsenceDto.SubjectId} not found");
            }

            // Get or determine semester
            Guid? semesterId = null;
            if (createAbsenceDto.SemesterId.HasValue)
            {
                var semester = await _unitOfWork.Semesters.GetByIdAsync(createAbsenceDto.SemesterId.Value);
                if (semester == null)
                    throw new NotFoundException($"Semester with ID {createAbsenceDto.SemesterId} not found");
                semesterId = createAbsenceDto.SemesterId.Value;
            }
            else
            {
                // Get current active semester for the teacher's institution
                var currentSemester = await _unitOfWork.Semesters.GetCurrentActiveAsync(teacher.InstitutionId);
                if (currentSemester != null)
                {
                    semesterId = currentSemester.Id;
                }
                // If no current semester, leave it null (for backwards compatibility)
            }

            // Create the absence entity
            var absence = new Absence
            {
                Id = Guid.NewGuid(),
                Date = DateTime.SpecifyKind(createAbsenceDto.Date, DateTimeKind.Utc),
                Status = createAbsenceDto.Status,
                Reason = createAbsenceDto.Reason ?? string.Empty,
                IsExcused = createAbsenceDto.IsExcused,
                StudentId = createAbsenceDto.StudentId,
                TeacherId = createAbsenceDto.TeacherId,
                SubjectId = createAbsenceDto.SubjectId ?? Guid.Empty,
                SemesterId = semesterId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Absences.AddAsync(absence, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            // Reload the absence with related entities
            absence = await _unitOfWork.Absences.GetByIdAsync(absence.Id, cancellationToken);
            if (absence == null)
                throw new NotFoundException($"Created absence with ID {absence.Id} could not be found");

            _logger.LogInformation("Created absence {AbsenceId} for student {StudentId} on {Date} in semester {SemesterId}", 
                absence.Id, createAbsenceDto.StudentId, createAbsenceDto.Date, semesterId);

            return AbsenceResponseDto.FromEntity(absence);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing absence record
    /// </summary>
    public async Task<AbsenceResponseDto> UpdateAsync(Guid id, UpdateAbsenceDto updateAbsenceDto, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var absence = await _unitOfWork.Absences.GetByIdAsync(id, cancellationToken);
            if (absence == null)
                throw new NotFoundException($"Absence with ID {id} not found");

            // Update properties if provided
            if (updateAbsenceDto.Date.HasValue)
                absence.Date = updateAbsenceDto.Date.Value;

            if (updateAbsenceDto.Status.HasValue)
                absence.Status = updateAbsenceDto.Status.Value;

            if (updateAbsenceDto.Reason != null)
                absence.Reason = updateAbsenceDto.Reason;

            if (updateAbsenceDto.IsExcused.HasValue)
                absence.IsExcused = updateAbsenceDto.IsExcused.Value;

            absence.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Absences.UpdateAsync(absence, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            // Reload the absence with related entities
            absence = await _unitOfWork.Absences.GetByIdAsync(id, cancellationToken);
            if (absence == null)
                throw new NotFoundException($"Updated absence with ID {id} could not be found");

            _logger.LogInformation("Updated absence {AbsenceId}", id);

            return AbsenceResponseDto.FromEntity(absence);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Deletes an absence record from the system
    /// </summary>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var absence = await _unitOfWork.Absences.GetByIdAsync(id, cancellationToken);
        if (absence == null)
            throw new NotFoundException($"Absence with ID {id} not found");

        await _unitOfWork.Absences.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted absence {AbsenceId}", id);
    }

    /// <summary>
    /// Gets attendance statistics for a student with optional semester filtering
    /// </summary>
    public async Task<StudentAttendanceStatsDto> GetStudentAttendanceStatsAsync(
        Guid studentId, 
        Guid? semesterId = null, 
        CancellationToken cancellationToken = default)
    {
        var absences = await GetByStudentIdAsync(studentId, semesterId, cancellationToken);
        var absencesList = absences.ToList();

        var totalAbsences = absencesList.Count;
        var excusedAbsences = absencesList.Count(a => a.IsExcused);
        var unexcusedAbsences = totalAbsences - excusedAbsences;

        // Calculate attendance rate (assuming standard school days)
        var semester = semesterId.HasValue 
            ? await _unitOfWork.Semesters.GetByIdAsync(semesterId.Value)
            : null;
        
        var totalSchoolDays = semester?.WeekCount * 5 ?? 180; // Default to 180 days if no semester info
        var attendedDays = Math.Max(0, totalSchoolDays - totalAbsences);
        var attendanceRate = totalSchoolDays > 0 ? (decimal)attendedDays / totalSchoolDays * 100 : 100;

        return new StudentAttendanceStatsDto
        {
            StudentId = studentId,
            SemesterId = semesterId,
            TotalAbsences = totalAbsences,
            ExcusedAbsences = excusedAbsences,
            UnexcusedAbsences = unexcusedAbsences,
            AttendanceRate = Math.Round(attendanceRate, 2),
            TotalSchoolDays = totalSchoolDays,
            AttendedDays = attendedDays
        };
    }

    /// <summary>
    /// Gets attendance summary for a subject with optional semester filtering
    /// </summary>
    public async Task<SubjectAttendanceSummaryDto> GetSubjectAttendanceSummaryAsync(
        Guid subjectId, 
        Guid? semesterId = null, 
        CancellationToken cancellationToken = default)
    {
        var absences = await GetBySubjectAsync(subjectId, semesterId, cancellationToken);
        var absencesList = absences.ToList();

        var totalAbsences = absencesList.Count;
        var excusedAbsences = absencesList.Count(a => a.IsExcused);
        var unexcusedAbsences = totalAbsences - excusedAbsences;

        // Get unique students who have absences
        var studentsWithAbsences = absencesList
            .GroupBy(a => a.StudentId)
            .Count();

        // Get total enrolled students for the subject (this would need implementation in repository)
        // For now, we'll use the students with absences as a baseline
        var totalStudents = studentsWithAbsences; // This should be replaced with actual enrollment count

        return new SubjectAttendanceSummaryDto
        {
            SubjectId = subjectId,
            SemesterId = semesterId,
            TotalStudents = totalStudents,
            StudentsWithAbsences = studentsWithAbsences,
            TotalAbsences = totalAbsences,
            ExcusedAbsences = excusedAbsences,
            UnexcusedAbsences = unexcusedAbsences,
            AverageAbsencesPerStudent = totalStudents > 0 ? (decimal)totalAbsences / totalStudents : 0
        };
    }
}







