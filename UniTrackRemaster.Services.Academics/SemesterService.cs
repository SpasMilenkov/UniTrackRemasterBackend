using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Semester;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Services.Academics;

public class SemesterService : ISemesterService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SemesterService> _logger;

    public SemesterService(IUnitOfWork unitOfWork, ILogger<SemesterService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    #region Basic CRUD operations

    public async Task<SemesterResponseDto> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving semester with ID: {SemesterId}", id);

        var semester = await _unitOfWork.Semesters.GetByIdAsync(id);
        if (semester == null)
        {
            throw new NotFoundException($"Semester with ID {id} not found.");
        }

        return SemesterResponseDto.FromEntity(semester);
    }

    public async Task<SemesterResponseDto> CreateAsync(CreateSemesterDto dto)
    {
        _logger.LogInformation("Creating semester: {Name} for academic year: {AcademicYearId}", 
            dto.Name, dto.AcademicYearId);

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Verify the academic year exists
            var academicYear = await _unitOfWork.AcademicYears.GetByIdAsync(dto.AcademicYearId);
            if (academicYear == null)
            {
                throw new NotFoundException($"Academic year with ID {dto.AcademicYearId} not found.");
            }

            // Validate the semester data
            await ValidateSemesterAsync(dto);

            // Create the semester entity
            var semester = new Semester
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Type = dto.Type,
                Status = dto.Status,
                WeekCount = dto.WeekCount > 0 ? dto.WeekCount : CalculateWeekCount(dto.StartDate, dto.EndDate),
                AcademicYearId = dto.AcademicYearId,
                RegistrationStartDate = dto.RegistrationStartDate,
                RegistrationEndDate = dto.RegistrationEndDate,
                AddDropDeadline = dto.AddDropDeadline,
                WithdrawalDeadline = dto.WithdrawalDeadline,
                MidtermStartDate = dto.MidtermStartDate,
                MidtermEndDate = dto.MidtermEndDate,
                FinalExamStartDate = dto.FinalExamStartDate,
                FinalExamEndDate = dto.FinalExamEndDate,
                GradeSubmissionDeadline = dto.GradeSubmissionDeadline
            };

            var newSemester = await _unitOfWork.Semesters.CreateAsync(semester);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Successfully created semester with ID: {SemesterId}", newSemester.Id);
            return SemesterResponseDto.FromEntity(newSemester);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error creating semester: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<SemesterResponseDto> UpdateAsync(Guid id, UpdateSemesterDto dto)
    {
        _logger.LogInformation("Updating semester with ID: {SemesterId}", id);

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var semester = await _unitOfWork.Semesters.GetByIdAsync(id);
            if (semester == null)
            {
                throw new NotFoundException($"Semester with ID {id} not found.");
            }

            // Update properties if provided
            if (dto.Name != null)
                semester.Name = dto.Name;
            
            if (dto.Description != null)
                semester.Description = dto.Description;
            
            if (dto.StartDate.HasValue)
                semester.StartDate = dto.StartDate.Value;
            
            if (dto.EndDate.HasValue)
                semester.EndDate = dto.EndDate.Value;
            
            if (dto.Type.HasValue)
                semester.Type = dto.Type.Value;
            
            if (dto.Status.HasValue)
                semester.Status = dto.Status.Value;
            
            if (dto.WeekCount.HasValue)
                semester.WeekCount = dto.WeekCount.Value;
            
            if (dto.RegistrationStartDate.HasValue)
                semester.RegistrationStartDate = dto.RegistrationStartDate;
            
            if (dto.RegistrationEndDate.HasValue)
                semester.RegistrationEndDate = dto.RegistrationEndDate;
            
            if (dto.AddDropDeadline.HasValue)
                semester.AddDropDeadline = dto.AddDropDeadline;
            
            if (dto.WithdrawalDeadline.HasValue)
                semester.WithdrawalDeadline = dto.WithdrawalDeadline;
            
            if (dto.MidtermStartDate.HasValue)
                semester.MidtermStartDate = dto.MidtermStartDate;
            
            if (dto.MidtermEndDate.HasValue)
                semester.MidtermEndDate = dto.MidtermEndDate;
            
            if (dto.FinalExamStartDate.HasValue)
                semester.FinalExamStartDate = dto.FinalExamStartDate;
            
            if (dto.FinalExamEndDate.HasValue)
                semester.FinalExamEndDate = dto.FinalExamEndDate;
            
            if (dto.GradeSubmissionDeadline.HasValue)
                semester.GradeSubmissionDeadline = dto.GradeSubmissionDeadline;

            // Validate the updated semester
            await ValidateSemesterAsync(semester, id);
            
            // Recalculate week count if dates changed but week count wasn't specified
            if ((dto.StartDate.HasValue || dto.EndDate.HasValue) && !dto.WeekCount.HasValue)
            {
                semester.WeekCount = CalculateWeekCount(semester.StartDate, semester.EndDate);
            }

            await _unitOfWork.Semesters.UpdateAsync(semester);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Successfully updated semester with ID: {SemesterId}", id);
            return SemesterResponseDto.FromEntity(semester);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error updating semester: {Message}", ex.Message);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting semester with ID: {SemesterId}", id);

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var semester = await _unitOfWork.Semesters.GetByIdAsync(id);
            if (semester == null)
            {
                throw new NotFoundException($"Semester with ID {id} not found.");
            }

            // Check if semester has associated courses or other dependencies
            bool hasCourses = await _unitOfWork.Semesters.HasCoursesAsync(id);
            if (hasCourses)
            {
                throw new ValidationException("Cannot delete semester with associated courses.");
            }

            await _unitOfWork.Semesters.DeleteAsync(id);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Successfully deleted semester with ID: {SemesterId}", id);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error deleting semester: {Message}", ex.Message);
            throw;
        }
    }

    #endregion

    #region Paginated methods with filtering (Academic Year Scoped)

    public async Task<PagedResult<SemesterResponseDto>> GetByAcademicYearAsync(
        Guid academicYearId,
        string? query = null,
        string? status = null,
        string? type = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 50)
    {
        _logger.LogInformation("Retrieving semesters for academic year: {AcademicYearId} with filters - Query: {Query}, Page: {Page}, PageSize: {PageSize}", 
            academicYearId, query, page, pageSize);
        
        // Validate pagination parameters
        if (page < 1)
        {
            throw new ArgumentException("Page number must be greater than 0", nameof(page));
        }
        
        if (pageSize < 1 || pageSize > 100)
        {
            throw new ArgumentException("Page size must be between 1 and 100", nameof(pageSize));
        }

        // Verify the academic year exists
        var academicYear = await _unitOfWork.AcademicYears.GetByIdAsync(academicYearId);
        if (academicYear == null)
        {
            throw new NotFoundException($"Academic year with ID {academicYearId} not found.");
        }

        try
        {
            var semesters = await _unitOfWork.Semesters.GetByAcademicYearAsync(
                academicYearId, query, status, type, isActive, page, pageSize);
            var totalCount = await _unitOfWork.Semesters.GetByAcademicYearCountAsync(
                academicYearId, query, status, type, isActive);
            
            var semesterDtos = semesters
                .Select(s => SemesterResponseDto.FromEntity(s))
                .ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            _logger.LogDebug("Successfully retrieved {Count} semesters for academic year {AcademicYearId} (Page {Page} of {TotalPages})", 
                semesterDtos.Count, academicYearId, page, totalPages);
            
            return new PagedResult<SemesterResponseDto>
            {
                Items = semesterDtos,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
            };
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex, "Unexpected error retrieving semesters for academic year {AcademicYearId}", academicYearId);
            throw;
        }
    }

    #endregion

    #region Non-paginated methods (for dropdowns, calculations)

    public async Task<IEnumerable<SemesterResponseDto>> GetAllByAcademicYearAsync(Guid academicYearId)
    {
        _logger.LogInformation("Retrieving all semesters for academic year: {AcademicYearId}", academicYearId);

        // Verify the academic year exists
        var academicYear = await _unitOfWork.AcademicYears.GetByIdAsync(academicYearId);
        if (academicYear == null)
        {
            throw new NotFoundException($"Academic year with ID {academicYearId} not found.");
        }

        var semesters = await _unitOfWork.Semesters.GetByAcademicYearAsync(academicYearId);
        return semesters.Select(SemesterResponseDto.FromEntity);
    }

    public async Task<SemesterResponseDto> GetCurrentAsync(Guid academicYearId)
    {
        _logger.LogInformation("Retrieving current semester for academic year: {AcademicYearId}", academicYearId);

        var currentSemester = await _unitOfWork.Semesters.GetCurrentAsync(academicYearId);
        if (currentSemester == null)
        {
            throw new NotFoundException($"No current semester found for academic year with ID {academicYearId}.");
        }

        return SemesterResponseDto.FromEntity(currentSemester);
    }

    public async Task<IEnumerable<SemesterResponseDto>> GetActiveAsync(Guid academicYearId)
    {
        _logger.LogInformation("Retrieving active semesters for academic year: {AcademicYearId}", academicYearId);

        var activeSemesters = await _unitOfWork.Semesters.GetActiveAsync(academicYearId);
        return activeSemesters.Select(SemesterResponseDto.FromEntity);
    }

    #endregion

    #region Global methods (Use Sparingly - Security Risk)

    public async Task<IEnumerable<SemesterResponseDto>> GetAllAsync()
    {
        _logger.LogWarning("Retrieving ALL semesters globally - security risk, use sparingly");

        var semesters = await _unitOfWork.Semesters.GetAllAsync();
        return semesters.Select(SemesterResponseDto.FromEntity);
    }

    public async Task<IEnumerable<SemesterResponseDto>> GetAllActiveAsync()
    {
        _logger.LogWarning("Retrieving ALL active semesters globally - security risk, use sparingly");

        var activeSemesters = await _unitOfWork.Semesters.GetActiveAsync();
        return activeSemesters.Select(SemesterResponseDto.FromEntity);
    }

    public async Task<IEnumerable<SemesterResponseDto>> GetByInstitutionAsync(Guid institutionId)
    {
        _logger.LogInformation("Retrieving semesters for institution: {InstitutionId}", institutionId);

        var semesters = await _unitOfWork.Semesters.GetByInstitutionAsync(institutionId);
        return semesters.Select(SemesterResponseDto.FromEntity);
    }

    public async Task<IEnumerable<SemesterResponseDto>> GetCurrentByInstitutionAsync(Guid institutionId)
    {
        _logger.LogInformation("Retrieving current semesters for institution: {InstitutionId}", institutionId);

        var currentSemesters = await _unitOfWork.Semesters.GetCurrentByInstitutionAsync(institutionId);
        return currentSemesters.Select(SemesterResponseDto.FromEntity);
    }

    #endregion

    #region Legacy methods (for backward compatibility)

    public async Task<IEnumerable<SemesterResponseDto>> GetByAcademicYearAsync(Guid academicYearId)
    {
        return await GetAllByAcademicYearAsync(academicYearId);
    }

    public async Task<IEnumerable<SemesterResponseDto>> GetActiveAsync()
    {
        return await GetAllActiveAsync();
    }

    #endregion

    #region Statistics and reporting

    public async Task<SemesterStatistics> GetStatisticsAsync(Guid id)
    {
        _logger.LogInformation("Retrieving statistics for semester: {SemesterId}", id);

        var statistics = await _unitOfWork.Semesters.GetStatisticsAsync(id);
        return statistics;
    }

    public async Task<int> GetTotalCountAsync(Guid academicYearId)
    {
        return await _unitOfWork.Semesters.GetByAcademicYearCountAsync(academicYearId);
    }

    #endregion

    #region Validation methods

    private async Task ValidateSemesterAsync(CreateSemesterDto dto)
    {
        // Basic date validation
        if (dto.StartDate >= dto.EndDate)
        {
            throw new ValidationException("Start date must be before end date.");
        }

        // Check for name uniqueness within academic year
        bool isNameUnique = await _unitOfWork.Semesters.IsNameUniqueAsync(dto.Name, dto.AcademicYearId);
        if (!isNameUnique)
        {
            throw new ValidationException($"A semester with the name '{dto.Name}' already exists in this academic year.");
        }

        // Check for date overlap within academic year
        bool hasDateOverlap = await _unitOfWork.Semesters.HasDateOverlapAsync(
            dto.AcademicYearId, dto.StartDate, dto.EndDate);
        if (hasDateOverlap)
        {
            throw new ValidationException("The specified date range overlaps with an existing semester.");
        }

        // Additional validation for optional date fields
        ValidateOptionalDateRanges(
            dto.StartDate, 
            dto.EndDate, 
            dto.RegistrationStartDate, 
            dto.RegistrationEndDate,
            dto.MidtermStartDate,
            dto.MidtermEndDate,
            dto.FinalExamStartDate,
            dto.FinalExamEndDate);

        // Validate semester duration (should be reasonable, e.g., between 1 week and 6 months)
        var duration = dto.EndDate - dto.StartDate;
        if (duration.TotalDays < 7 || duration.TotalDays > 180)
        {
            throw new ValidationException("Semester duration must be between 1 week and 6 months.");
        }
    }

    private async Task ValidateSemesterAsync(Semester semester, Guid excludeId)
    {
        // Basic date validation
        if (semester.StartDate >= semester.EndDate)
        {
            throw new ValidationException("Start date must be before end date.");
        }

        // Check for name uniqueness within academic year
        bool isNameUnique = await _unitOfWork.Semesters.IsNameUniqueAsync(
            semester.Name, semester.AcademicYearId, excludeId);
        if (!isNameUnique)
        {
            throw new ValidationException($"A semester with the name '{semester.Name}' already exists in this academic year.");
        }

        // Check for date overlap within academic year
        bool hasDateOverlap = await _unitOfWork.Semesters.HasDateOverlapAsync(
            semester.AcademicYearId, semester.StartDate, semester.EndDate, excludeId);
        if (hasDateOverlap)
        {
            throw new ValidationException("The specified date range overlaps with an existing semester.");
        }

        // Additional validation for optional date fields
        ValidateOptionalDateRanges(
            semester.StartDate, 
            semester.EndDate, 
            semester.RegistrationStartDate, 
            semester.RegistrationEndDate,
            semester.MidtermStartDate,
            semester.MidtermEndDate,
            semester.FinalExamStartDate,
            semester.FinalExamEndDate);

        // Validate semester duration
        var duration = semester.EndDate - semester.StartDate;
        if (duration.TotalDays < 7 || duration.TotalDays > 180)
        {
            throw new ValidationException("Semester duration must be between 1 week and 6 months.");
        }
    }

    private void ValidateOptionalDateRanges(
        DateTime startDate, 
        DateTime endDate, 
        DateTime? registrationStartDate,
        DateTime? registrationEndDate,
        DateTime? midtermStartDate,
        DateTime? midtermEndDate,
        DateTime? finalExamStartDate,
        DateTime? finalExamEndDate)
    {
        // Registration dates
        if (registrationStartDate.HasValue && registrationEndDate.HasValue)
        {
            if (registrationStartDate.Value > registrationEndDate.Value)
            {
                throw new ValidationException("Registration start date must be before registration end date.");
            }
        }

        // Midterm dates
        if (midtermStartDate.HasValue && midtermEndDate.HasValue)
        {
            if (midtermStartDate.Value > midtermEndDate.Value)
            {
                throw new ValidationException("Midterm start date must be before midterm end date.");
            }

            if (midtermStartDate.Value < startDate || midtermEndDate.Value > endDate)
            {
                throw new ValidationException("Midterm dates must be within the semester date range.");
            }
        }

        // Final exam dates
        if (finalExamStartDate.HasValue && finalExamEndDate.HasValue)
        {
            if (finalExamStartDate.Value > finalExamEndDate.Value)
            {
                throw new ValidationException("Final exam start date must be before final exam end date.");
            }

            if (finalExamStartDate.Value < startDate || finalExamEndDate.Value > endDate)
            {
                throw new ValidationException("Final exam dates must be within the semester date range.");
            }
        }
    }

    #endregion

    #region Status management

    public async Task SetStatusAsync(Guid id, SemesterStatus status)
    {
        _logger.LogInformation("Setting semester {SemesterId} status to: {Status}", id, status);

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var semester = await _unitOfWork.Semesters.GetByIdAsync(id);
            if (semester == null)
            {
                throw new NotFoundException($"Semester with ID {id} not found.");
            }

            semester.Status = status;

            await _unitOfWork.Semesters.UpdateAsync(semester);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Successfully updated status for semester {SemesterId}", id);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error updating status for semester {SemesterId}", id);
            throw;
        }
    }

    #endregion

    #region Helper methods

    private int CalculateWeekCount(DateTime startDate, DateTime endDate)
    {
        // Calculate the difference in days and convert to weeks (rounded up)
        TimeSpan duration = endDate - startDate;
        return (int)Math.Ceiling(duration.TotalDays / 7.0);
    }

    #endregion
}