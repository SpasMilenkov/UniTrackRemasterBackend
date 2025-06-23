using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.AcademicYear;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Services.Academics;

public class AcademicYearService : IAcademicYearService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AcademicYearService> _logger;

    public AcademicYearService(IUnitOfWork unitOfWork, ILogger<AcademicYearService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    #region Basic CRUD operations

    public async Task<AcademicYearResponseDto> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving academic year with ID: {AcademicYearId}", id);

        var academicYear = await _unitOfWork.AcademicYears.GetByIdAsync(id);
        if (academicYear == null)
        {
            throw new NotFoundException($"Academic year with ID {id} not found.");
        }

        return AcademicYearResponseDto.FromEntity(academicYear);
    }

    public async Task<AcademicYearResponseDto> CreateAsync(CreateAcademicYearDto dto)
    {
        _logger.LogInformation("Creating academic year: {Name} for institution: {InstitutionId}", 
            dto.Name, dto.InstitutionId);

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Validate the academic year data
            await ValidateAcademicYearAsync(dto);

            // Create the academic year entity
            var academicYear = new AcademicYear
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                InstitutionId = dto.InstitutionId,
                Semesters = new List<Semester>()
            };

            var newAcademicYear = await _unitOfWork.AcademicYears.CreateAsync(academicYear);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Successfully created academic year with ID: {AcademicYearId}", newAcademicYear.Id);
            return AcademicYearResponseDto.FromEntity(newAcademicYear);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error creating academic year: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<AcademicYearResponseDto> UpdateAsync(Guid id, UpdateAcademicYearDto dto)
    {
        _logger.LogInformation("Updating academic year with ID: {AcademicYearId}", id);

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var academicYear = await _unitOfWork.AcademicYears.GetByIdAsync(id);
            if (academicYear == null)
            {
                throw new NotFoundException($"Academic year with ID {id} not found.");
            }

            // Update properties if provided
            if (dto.Name != null)
                academicYear.Name = dto.Name;
            
            if (dto.StartDate.HasValue)
                academicYear.StartDate = dto.StartDate.Value;
            
            if (dto.EndDate.HasValue)
                academicYear.EndDate = dto.EndDate.Value;

            // Validate the updated academic year
            await ValidateAcademicYearAsync(academicYear, id);

            await _unitOfWork.AcademicYears.UpdateAsync(academicYear);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Successfully updated academic year with ID: {AcademicYearId}", id);
            return AcademicYearResponseDto.FromEntity(academicYear);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error updating academic year: {Message}", ex.Message);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting academic year with ID: {AcademicYearId}", id);

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var academicYear = await _unitOfWork.AcademicYears.GetByIdAsync(id);
            if (academicYear == null)
            {
                throw new NotFoundException($"Academic year with ID {id} not found.");
            }

            // Check if academic year has associated semesters
            bool hasSemesters = await _unitOfWork.AcademicYears.HasSemestersAsync(id);
            if (hasSemesters)
            {
                throw new ValidationException("Cannot delete academic year with associated semesters.");
            }

            await _unitOfWork.AcademicYears.DeleteAsync(id);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Successfully deleted academic year with ID: {AcademicYearId}", id);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error deleting academic year: {Message}", ex.Message);
            throw;
        }
    }

    #endregion

    #region Paginated methods with filtering

    public async Task<PagedResult<AcademicYearResponseDto>> GetByInstitutionAsync(
        Guid institutionId,
        string? query = null,
        bool? isActive = null,
        bool? isCurrent = null,
        int page = 1,
        int pageSize = 50)
    {
        _logger.LogInformation("Retrieving academic years for institution: {InstitutionId} with filters - Query: {Query}, Page: {Page}, PageSize: {PageSize}", 
            institutionId, query, page, pageSize);
        
        // Validate pagination parameters
        if (page < 1)
        {
            throw new ArgumentException("Page number must be greater than 0", nameof(page));
        }
        
        if (pageSize < 1 || pageSize > 100)
        {
            throw new ArgumentException("Page size must be between 1 and 100", nameof(pageSize));
        }

        // Verify institution exists (basic check)
        // You might want to add a proper institution existence check here

        try
        {
            var academicYears = await _unitOfWork.AcademicYears.GetByInstitutionAsync(
                institutionId, query, isActive, isCurrent, page, pageSize);
            var totalCount = await _unitOfWork.AcademicYears.GetByInstitutionCountAsync(
                institutionId, query, isActive, isCurrent);
            
            var academicYearDtos = academicYears
                .Select(ay => AcademicYearResponseDto.FromEntity(ay))
                .ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            _logger.LogDebug("Successfully retrieved {Count} academic years for institution {InstitutionId} (Page {Page} of {TotalPages})", 
                academicYearDtos.Count, institutionId, page, totalPages);
            
            return new PagedResult<AcademicYearResponseDto>
            {
                Items = academicYearDtos,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
            };
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex, "Unexpected error retrieving academic years for institution {InstitutionId}", institutionId);
            throw;
        }
    }

    #endregion

    #region Non-paginated methods (for dropdowns, calculations)

    public async Task<IEnumerable<AcademicYearResponseDto>> GetAllByInstitutionAsync(Guid institutionId)
    {
        _logger.LogInformation("Retrieving all academic years for institution: {InstitutionId}", institutionId);

        var academicYears = await _unitOfWork.AcademicYears.GetAllAsync(institutionId);
        return academicYears.Select(AcademicYearResponseDto.FromEntity);
    }

    public async Task<IEnumerable<AcademicYearResponseDto>> GetByInstitutionAsync(Guid institutionId)
    {
        _logger.LogInformation("Retrieving academic years for institution: {InstitutionId}", institutionId);

        var academicYears = await _unitOfWork.AcademicYears.GetByInstitutionAsync(institutionId);
        return academicYears.Select(AcademicYearResponseDto.FromEntity);
    }

    public async Task<AcademicYearResponseDto> GetCurrentAsync(Guid institutionId)
    {
        _logger.LogInformation("Retrieving current academic year for institution: {InstitutionId}", institutionId);

        var currentAcademicYear = await _unitOfWork.AcademicYears.GetCurrentAsync(institutionId);
        if (currentAcademicYear == null)
        {
            throw new NotFoundException($"No current academic year found for institution with ID {institutionId}.");
        }

        return AcademicYearResponseDto.FromEntity(currentAcademicYear);
    }

    public async Task<IEnumerable<AcademicYearResponseDto>> GetActiveAsync(Guid institutionId)
    {
        _logger.LogInformation("Retrieving active academic years for institution: {InstitutionId}", institutionId);

        var activeAcademicYears = await _unitOfWork.AcademicYears.GetActiveAsync(institutionId);
        return activeAcademicYears.Select(AcademicYearResponseDto.FromEntity);
    }

    #endregion

    #region Statistics and reporting

    public async Task<AcademicYearStatistics> GetStatisticsAsync(Guid id)
    {
        _logger.LogInformation("Retrieving statistics for academic year: {AcademicYearId}", id);

        var statistics = await _unitOfWork.AcademicYears.GetStatisticsAsync(id);
        return statistics;
    }

    public async Task<int> GetTotalCountAsync(Guid institutionId)
    {
        return await _unitOfWork.AcademicYears.GetByInstitutionCountAsync(institutionId);
    }

    #endregion

    #region Validation methods

    private async Task ValidateAcademicYearAsync(CreateAcademicYearDto dto)
    {
        // Validate date range
        if (dto.StartDate >= dto.EndDate)
        {
            throw new ValidationException("Start date must be before end date.");
        }

        // Check for name uniqueness within institution
        bool isNameUnique = await _unitOfWork.AcademicYears.IsNameUniqueAsync(dto.Name, dto.InstitutionId);
        if (!isNameUnique)
        {
            throw new ValidationException($"An academic year with the name '{dto.Name}' already exists in this institution.");
        }

        // Check for date overlap within institution
        bool hasDateOverlap = await _unitOfWork.AcademicYears.HasDateOverlapAsync(
            dto.InstitutionId, dto.StartDate, dto.EndDate);
        if (hasDateOverlap)
        {
            throw new ValidationException("The specified date range overlaps with an existing academic year.");
        }

        // Validate academic year duration (should be reasonable, e.g., between 6 months and 2 years)
        var duration = dto.EndDate - dto.StartDate;
        if (duration.TotalDays < 180 || duration.TotalDays > 730)
        {
            throw new ValidationException("Academic year duration must be between 6 months and 2 years.");
        }
    }

    private async Task ValidateAcademicYearAsync(AcademicYear academicYear, Guid excludeId)
    {
        // Validate date range
        if (academicYear.StartDate >= academicYear.EndDate)
        {
            throw new ValidationException("Start date must be before end date.");
        }

        // Check for name uniqueness within institution
        bool isNameUnique = await _unitOfWork.AcademicYears.IsNameUniqueAsync(
            academicYear.Name, academicYear.InstitutionId, excludeId);
        if (!isNameUnique)
        {
            throw new ValidationException($"An academic year with the name '{academicYear.Name}' already exists in this institution.");
        }

        // Check for date overlap within institution
        bool hasDateOverlap = await _unitOfWork.AcademicYears.HasDateOverlapAsync(
            academicYear.InstitutionId, academicYear.StartDate, academicYear.EndDate, excludeId);
        if (hasDateOverlap)
        {
            throw new ValidationException("The specified date range overlaps with an existing academic year.");
        }

        // Validate academic year duration
        var duration = academicYear.EndDate - academicYear.StartDate;
        if (duration.TotalDays < 180 || duration.TotalDays > 730)
        {
            throw new ValidationException("Academic year duration must be between 6 months and 2 years.");
        }
    }

    #endregion
}