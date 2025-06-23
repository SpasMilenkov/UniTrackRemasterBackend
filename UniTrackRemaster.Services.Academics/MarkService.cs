using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Mark;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Services.Organization;

namespace UniTrackRemaster.Services.Academics;

/// <summary>
/// Service for managing marks with grading system integration and semester support.
/// Handles conversion between institution-specific grades and standardized 0-100 scores.
/// </summary>
public class MarkService : IMarkService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGradingSystemService _gradingSystemService;
    private readonly ILogger<MarkService> _logger;

    public MarkService(
        IUnitOfWork unitOfWork, 
        IGradingSystemService gradingSystemService,
        ILogger<MarkService> logger)
    {
        _unitOfWork = unitOfWork;
        _gradingSystemService = gradingSystemService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all marks with grading system enhancements
    /// </summary>
    public async Task<IEnumerable<MarkResponseDto>> GetAllMarksAsync(CancellationToken cancellationToken = default)
    {
        var marks = await _unitOfWork.Marks.GetAllAsync(cancellationToken);
        var enhancedMarks = new List<MarkResponseDto>();

        foreach (var mark in marks)
        {
            var enhancedMark = await EnhanceMarkWithGradingSystemAsync(mark);
            enhancedMarks.Add(enhancedMark);
        }

        return enhancedMarks;
    }

    /// <summary>
    /// Retrieves a mark by ID with grading system enhancements
    /// </summary>
    public async Task<MarkResponseDto> GetMarkByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var mark = await _unitOfWork.Marks.GetByIdAsync(id, cancellationToken);
            
        if (mark == null)
            throw new NotFoundException($"Mark with ID {id} not found");

        return await EnhanceMarkWithGradingSystemAsync(mark);
    }

    /// <summary>
    /// Retrieves marks by student ID with optional semester filtering and grading system enhancements
    /// </summary>
    public async Task<IEnumerable<MarkResponseDto>> GetMarksByStudentIdAsync(
        Guid studentId, 
        Guid? semesterId = null, 
        CancellationToken cancellationToken = default)
    {
        var marks = semesterId.HasValue 
            ? await _unitOfWork.Marks.GetMarksByStudentIdAsync(studentId, semesterId, cancellationToken)
            : await _unitOfWork.Marks.GetMarksByStudentIdAsync(studentId, cancellationToken);

        var enhancedMarks = new List<MarkResponseDto>();
        foreach (var mark in marks)
        {
            var enhancedMark = await EnhanceMarkWithGradingSystemAsync(mark);
            enhancedMarks.Add(enhancedMark);
        }

        return enhancedMarks;
    }

    /// <summary>
    /// Retrieves marks by teacher ID with optional semester filtering and grading system enhancements
    /// </summary>
    public async Task<IEnumerable<MarkResponseDto>> GetMarksByTeacherIdAsync(
        Guid teacherId, 
        Guid? semesterId = null, 
        CancellationToken cancellationToken = default)
    {
        var marks = await _unitOfWork.Marks.GetMarksByTeacherIdAsync(teacherId, semesterId, cancellationToken);
        var enhancedMarks = new List<MarkResponseDto>();

        foreach (var mark in marks)
        {
            var enhancedMark = await EnhanceMarkWithGradingSystemAsync(mark);
            enhancedMarks.Add(enhancedMark);
        }

        return enhancedMarks;
    }

    /// <summary>
    /// Retrieves marks by subject ID with optional semester filtering and grading system enhancements
    /// </summary>
    public async Task<IEnumerable<MarkResponseDto>> GetMarksBySubjectIdAsync(
        Guid subjectId, 
        Guid? semesterId = null, 
        CancellationToken cancellationToken = default)
    {
        var marks = await _unitOfWork.Marks.GetMarksBySubjectIdAsync(subjectId, semesterId, cancellationToken);
        var enhancedMarks = new List<MarkResponseDto>();

        foreach (var mark in marks)
        {
            var enhancedMark = await EnhanceMarkWithGradingSystemAsync(mark);
            enhancedMarks.Add(enhancedMark);
        }

        return enhancedMarks;
    }

    /// <summary>
    /// Retrieves marks by filter parameters with grading system enhancements
    /// </summary>
    public async Task<IEnumerable<MarkResponseDto>> GetMarksByFilterAsync(
        MarkFilterParams filterParams, 
        CancellationToken cancellationToken = default)
    {
        var marks = await _unitOfWork.Marks.GetMarksByFilterAsync(filterParams, cancellationToken);
        var enhancedMarks = new List<MarkResponseDto>();

        foreach (var mark in marks)
        {
            var enhancedMark = await EnhanceMarkWithGradingSystemAsync(mark);
            enhancedMarks.Add(enhancedMark);
        }

        return enhancedMarks;
    }

    /// <summary>
    /// Creates a new mark with automatic grading system conversion and semester context
    /// </summary>
    public async Task<MarkResponseDto> CreateMarkAsync(CreateMarkDto createMarkDto, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Validate existence of related entities
            var student = await _unitOfWork.Students.GetByIdAsync(createMarkDto.StudentId);
            if (student == null)
                throw new NotFoundException($"Student with ID {createMarkDto.StudentId} not found");
                
            var teacher = await _unitOfWork.Teachers.GetByIdAsync(createMarkDto.TeacherId);
            if (teacher == null)
                throw new NotFoundException($"Teacher with ID {createMarkDto.TeacherId} not found");
                
            var subject = await _unitOfWork.Subjects.GetByIdAsync(createMarkDto.SubjectId);
            if (subject == null)
                throw new NotFoundException($"Subject with ID {createMarkDto.SubjectId} not found");

            // Get or determine semester
            Guid semesterId;
            if (createMarkDto.SemesterId.HasValue)
            {
                var semester = await _unitOfWork.Semesters.GetByIdAsync(createMarkDto.SemesterId.Value);
                if (semester == null)
                    throw new NotFoundException($"Semester with ID {createMarkDto.SemesterId} not found");
                semesterId = createMarkDto.SemesterId.Value;
            }
            else
            {
                // Get current active semester for the teacher's institution
                var currentSemester = await _unitOfWork.Semesters.GetCurrentActiveAsync(teacher.InstitutionId);
                if (currentSemester == null)
                    throw new NotFoundException("No active semester found for the teacher's institution");
                semesterId = currentSemester.Id;
            }

            // Get institution's grading system
            var gradingSystemDto = await _gradingSystemService.GetDefaultForInstitutionAsync(teacher.InstitutionId);
            
            // Convert grade to 0-100 score if grade is provided, otherwise use direct score
            decimal scoreValue;
            if (!string.IsNullOrWhiteSpace(createMarkDto.Grade))
            {
                // Convert institution-specific grade to 0-100 score
                scoreValue = _gradingSystemService.ConvertGradeToScore(createMarkDto.Grade, gradingSystemDto.Id);
                
                if (scoreValue < 0 || scoreValue > 100)
                    throw new ValidationException($"Invalid grade '{createMarkDto.Grade}' for the institution's grading system");
            }
            else if (createMarkDto.Score.HasValue)
            {
                // Use direct score
                scoreValue = createMarkDto.Score.Value;
            }
            else
            {
                throw new ValidationException("Either Grade or Score must be provided");
            }

            // Create the mark entity with the converted score
            var mark = new Mark
            {
                Id = Guid.NewGuid(),
                Value = scoreValue, // Store as 0-100 score
                Topic = createMarkDto.Topic,
                Description = createMarkDto.Description,
                Type = createMarkDto.Type,
                SubjectId = createMarkDto.SubjectId,
                TeacherId = createMarkDto.TeacherId,
                StudentId = createMarkDto.StudentId,
                SemesterId = semesterId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            await _unitOfWork.Marks.AddAsync(mark, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            
            // Reload the mark with related entities
            mark = await _unitOfWork.Marks.GetByIdAsync(mark.Id, cancellationToken);
            if (mark == null)
                throw new NotFoundException($"Created mark with ID {mark.Id} could not be found");
                
            _logger.LogInformation("Created mark {MarkId} for student {StudentId} with score {Score} (converted from grade '{Grade}')", 
                mark.Id, createMarkDto.StudentId, scoreValue, createMarkDto.Grade ?? "direct score");

            return await EnhanceMarkWithGradingSystemAsync(mark);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing mark with grading system conversion
    /// </summary>
    public async Task<MarkResponseDto> UpdateMarkAsync(Guid id, UpdateMarkDto updateMarkDto, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var mark = await _unitOfWork.Marks.GetByIdAsync(id, cancellationToken);
            if (mark == null)
                throw new NotFoundException($"Mark with ID {id} not found");

            // Get the teacher's institution for grading system context
            var teacher = await _unitOfWork.Teachers.GetByIdAsync(mark.TeacherId);
            if (teacher == null)
                throw new NotFoundException($"Teacher with ID {mark.TeacherId} not found");

            // Get institution's grading system
            var gradingSystemDto = await _gradingSystemService.GetDefaultForInstitutionAsync(teacher.InstitutionId);
                
            // Update score if grade or score is provided
            if (!string.IsNullOrWhiteSpace(updateMarkDto.Grade))
            {
                // Convert institution-specific grade to 0-100 score
                var scoreValue = _gradingSystemService.ConvertGradeToScore(updateMarkDto.Grade, gradingSystemDto.Id);
                
                if (scoreValue < 0 || scoreValue > 100)
                    throw new ValidationException($"Invalid grade '{updateMarkDto.Grade}' for the institution's grading system");
                    
                mark.Value = scoreValue;
            }
            else if (updateMarkDto.Score.HasValue)
            {
                // Use direct score
                mark.Value = updateMarkDto.Score.Value;
            }
                
            // Update other properties that are provided
            if (!string.IsNullOrWhiteSpace(updateMarkDto.Topic))
                mark.Topic = updateMarkDto.Topic;
                
            mark.Description = updateMarkDto.Description; // Can be set to null
            
            if (updateMarkDto.Type.HasValue)
                mark.Type = updateMarkDto.Type.Value;

            mark.UpdatedAt = DateTime.UtcNow;
                
            await _unitOfWork.Marks.UpdateAsync(mark, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            
            // Reload the mark with related entities
            mark = await _unitOfWork.Marks.GetByIdAsync(id, cancellationToken);
            if (mark == null)
                throw new NotFoundException($"Updated mark with ID {id} could not be found");

            _logger.LogInformation("Updated mark {MarkId} with new score {Score} (converted from grade '{Grade}')", 
                id, mark.Value, updateMarkDto.Grade ?? "direct score");
                
            return await EnhanceMarkWithGradingSystemAsync(mark);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Deletes a mark from the system
    /// </summary>
    public async Task DeleteMarkAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var mark = await _unitOfWork.Marks.GetByIdAsync(id, cancellationToken);
        if (mark == null)
            throw new NotFoundException($"Mark with ID {id} not found");
            
        await _unitOfWork.Marks.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted mark {MarkId}", id);
    }

    /// <summary>
    /// Gets average mark for a student with optional subject and semester filtering
    /// </summary>
    public async Task<decimal> GetAverageMarkForStudentAsync(
        Guid studentId, 
        Guid? subjectId = null, 
        Guid? semesterId = null, 
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Marks.GetAverageMarkForStudentAsync(studentId, subjectId, semesterId, cancellationToken);
    }

    /// <summary>
    /// Gets average mark for a subject with optional semester filtering
    /// </summary>
    public async Task<decimal> GetAverageMarkForSubjectAsync(
        Guid subjectId, 
        Guid? semesterId = null, 
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Marks.GetAverageMarkForSubjectAsync(subjectId, semesterId, cancellationToken);
    }

    /// <summary>
    /// Gets GPA for a student with optional semester filtering
    /// </summary>
    public async Task<double> GetStudentGpaAsync(
        Guid studentId, 
        Guid? semesterId = null, 
        CancellationToken cancellationToken = default)
    {
        // Get student to determine institution
        var student = await _unitOfWork.Students.GetByIdAsync(studentId);
        if (student == null)
            throw new NotFoundException($"Student with ID {studentId} not found");

        // Get marks for the student
        var marks = await GetMarksByStudentIdAsync(studentId, semesterId, cancellationToken);
        
        if (!marks.Any())
            return 0.0;

        // Calculate GPA using the institution's grading system
        var totalGpaPoints = marks.Sum(m => m.GpaPoints ?? 0);
        var totalCredits = marks.Count(); // Assuming each mark has equal weight

        return totalCredits > 0 ? totalGpaPoints / totalCredits : 0.0;
    }

    /// <summary>
    /// Helper method to enhance mark with grading system information
    /// </summary>
    private async Task<MarkResponseDto> EnhanceMarkWithGradingSystemAsync(Mark mark)
    {
        var markDto = MarkResponseDto.FromEntity(mark);
        
        try
        {
            // Get the teacher's institution to determine grading system
            var teacher = await _unitOfWork.Teachers.GetByIdAsync(mark.TeacherId);
            if (teacher != null)
            {
                // Get institution's grading system
                var gradingSystemDto = await _gradingSystemService.GetDefaultForInstitutionAsync(teacher.InstitutionId);
                
                // Enhance with grading system information
                markDto.Grade = _gradingSystemService.ConvertScoreToGrade(mark.Value, gradingSystemDto.Id);
                markDto.GpaPoints = _gradingSystemService.ConvertScoreToGpaPoints(mark.Value, gradingSystemDto.Id);
                markDto.Status = _gradingSystemService.DetermineStatus(mark.Value, gradingSystemDto.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to enhance mark {MarkId} with grading system information", mark.Id);
            // Continue without grading system enhancements
        }
        
        return markDto;
    }
}