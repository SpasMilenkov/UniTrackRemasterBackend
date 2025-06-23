using UniTrackRemaster.Api.Dto.Grading;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Services.Organization.Strategies;

namespace UniTrackRemaster.Services.Organization;

public class GradingSystemService : IGradingSystemService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly GradingStrategyFactory _strategyFactory;

    public GradingSystemService(IUnitOfWork unitOfWork, GradingStrategyFactory strategyFactory)
    {
        _unitOfWork = unitOfWork;
        _strategyFactory = strategyFactory;
    }

    public async Task<GradingSystemResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var gradingSystem = await _unitOfWork.GradingSystems.GetWithGradeScalesAsync(id, cancellationToken);
        if (gradingSystem == null)
        {
            throw new KeyNotFoundException($"Grading system with ID {id} not found");
        }

        return GradingSystemResponseDto.FromEntity(gradingSystem);
    }

    public async Task<IEnumerable<GradingSystemResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var gradingSystems = await _unitOfWork.GradingSystems.GetAllAsync(cancellationToken);
        return gradingSystems.Select(GradingSystemResponseDto.FromEntity);
    }

    public async Task<IEnumerable<GradingSystemResponseDto>> GetAllForInstitutionAsync(Guid institutionId, CancellationToken cancellationToken = default)
    {
        var gradingSystems = await _unitOfWork.GradingSystems.GetAllForInstitutionAsync(institutionId, cancellationToken);
        return gradingSystems.Select(GradingSystemResponseDto.FromEntity);
    }

    public async Task<GradingSystemResponseDto> GetDefaultForInstitutionAsync(Guid institutionId, CancellationToken cancellationToken = default)
    {
        var gradingSystem = await _unitOfWork.GradingSystems.GetDefaultForInstitutionAsync(institutionId, cancellationToken);
        if (gradingSystem == null)
        {
            throw new KeyNotFoundException($"No default grading system found for institution {institutionId}");
        }

        return GradingSystemResponseDto.FromEntity(gradingSystem);
    }

    public async Task<GradingSystemResponseDto> CreateAsync(CreateGradingSystemDto dto, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Check if a grading system with the same name already exists for this institution
            var existingSystem = await _unitOfWork.GradingSystems
                .GetByNameAndInstitutionAsync(dto.Name, dto.InstitutionId, cancellationToken);

            if (existingSystem != null)
            {
                throw new InvalidOperationException($"A grading system with the name '{dto.Name}' already exists for this institution.");
            }

            // Create the grading system entity
            var gradingSystem = CreateGradingSystemDto.ToEntity(dto);

            // If this is being set as default, update any existing defaults
            if (dto.IsDefault)
            {
                var existingSystems = await _unitOfWork.GradingSystems
                    .GetAllForInstitutionAsync(dto.InstitutionId, cancellationToken);

                foreach (var system in existingSystems.Where(gs => gs.IsDefault))
                {
                    system.IsDefault = false;
                    await _unitOfWork.GradingSystems.UpdateAsync(system, cancellationToken);
                }
            }

            // Add the new grading system
            await _unitOfWork.GradingSystems.AddAsync(gradingSystem, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            // Fetch the complete entity with its related data
            var createdSystem = await _unitOfWork.GradingSystems.GetWithGradeScalesAsync(gradingSystem.Id, cancellationToken);

            // Return the created system
            return GradingSystemResponseDto.FromEntity(createdSystem);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<GradingSystemResponseDto> UpdateAsync(Guid id, UpdateGradingSystemDto dto, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var gradingSystem = await _unitOfWork.GradingSystems.GetWithGradeScalesAsync(id, cancellationToken);
            if (gradingSystem == null)
            {
                throw new KeyNotFoundException($"Grading system with ID {id} not found");
            }

            // Update base properties if provided in the DTO
            if (!string.IsNullOrEmpty(dto.Name))
            {
                gradingSystem.Name = dto.Name;
            }

            if (dto.Description != null)
            {
                gradingSystem.Description = dto.Description;
            }

            if (dto.MinimumPassingScore.HasValue)
            {
                gradingSystem.MinimumPassingScore = dto.MinimumPassingScore.Value;
            }

            if (dto.MaximumScore.HasValue)
            {
                gradingSystem.MaximumScore = dto.MaximumScore.Value;
            }

            // Handle IsDefault flag
            if (dto.IsDefault.HasValue && dto.IsDefault.Value && !gradingSystem.IsDefault)
            {
                // Set as default and update other systems
                await _unitOfWork.GradingSystems.SetDefaultAsync(id, gradingSystem.InstitutionId, cancellationToken);
            }
            else if (dto.IsDefault.HasValue)
            {
                gradingSystem.IsDefault = dto.IsDefault.Value;
            }

            // Update grade scales if provided
            if (dto.GradeScales != null && dto.GradeScales.Any())
            {
                // Clear existing scales - the repository will handle this
                gradingSystem.GradeScales.Clear();

                // Add new scales
                foreach (var scaleDto in dto.GradeScales)
                {
                    gradingSystem.GradeScales.Add(GradeScaleDto.ToEntity(scaleDto, id));
                }
            }

            // Update and save
            var updatedSystem = await _unitOfWork.GradingSystems.UpdateAsync(gradingSystem, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            // Fetch the complete updated entity
            var fetchedSystem = await _unitOfWork.GradingSystems.GetWithGradeScalesAsync(id, cancellationToken);

            return GradingSystemResponseDto.FromEntity(fetchedSystem);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await _unitOfWork.GradingSystems.DeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return result;
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> SetDefaultAsync(Guid id, Guid institutionId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.GradingSystems.SetDefaultAsync(id, institutionId, cancellationToken);
    }

    public async Task<bool> InitializeDefaultGradingSystemsAsync(Guid institutionId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await InitializeDefaultGradingSystemsWithinTransactionAsync(institutionId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            return result;
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Initializes default grading systems for an institution within an existing transaction context.
    /// This method does NOT start or commit transactions - it assumes one is already active.
    /// </summary>
    /// <param name="institutionId">The ID of the institution</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if grading systems were created, false if they already existed</returns>
    public async Task<bool> InitializeDefaultGradingSystemsWithinTransactionAsync(Guid institutionId, CancellationToken cancellationToken = default)
    {
        // Check if institution already has grading systems
        var existingSystems = await _unitOfWork.GradingSystems.GetAllForInstitutionAsync(institutionId, cancellationToken);
        if (existingSystems.Any())
        {
            // Already initialized, so return false
            return false;
        }

        // Create the default grading systems
        var strategies = new List<IGradingStrategy>
        {
            _strategyFactory.GetStrategy(GradingSystemType.American),
            _strategyFactory.GetStrategy(GradingSystemType.European),
            _strategyFactory.GetStrategy(GradingSystemType.Bulgarian)
        };

        foreach (var strategy in strategies)
        {
            var gradingSystem = strategy.CreateDefaultGradingSystem(institutionId);
            await _unitOfWork.GradingSystems.AddAsync(gradingSystem, cancellationToken);
        }

        return true;
    }

    // Grade conversion methods
    public string ConvertScoreToGrade(decimal score, Guid gradingSystemId)
    {
        var gradingSystem = _unitOfWork.GradingSystems.GetWithGradeScalesAsync(gradingSystemId).GetAwaiter().GetResult();
        if (gradingSystem == null)
        {
            throw new KeyNotFoundException($"Grading system with ID {gradingSystemId} not found");
        }

        var strategy = _strategyFactory.GetStrategy(gradingSystem);
        return strategy.ConvertScoreToGrade(score);
    }

    public double ConvertScoreToGpaPoints(decimal score, Guid gradingSystemId)
    {
        var gradingSystem = _unitOfWork.GradingSystems.GetWithGradeScalesAsync(gradingSystemId).GetAwaiter().GetResult();
        if (gradingSystem == null)
        {
            throw new KeyNotFoundException($"Grading system with ID {gradingSystemId} not found");
        }

        var strategy = _strategyFactory.GetStrategy(gradingSystem);
        return strategy.ConvertScoreToGpaPoints(score);
    }

    public string DetermineStatus(decimal score, Guid gradingSystemId)
    {
        var gradingSystem = _unitOfWork.GradingSystems.GetWithGradeScalesAsync(gradingSystemId).GetAwaiter().GetResult();
        if (gradingSystem == null)
        {
            throw new KeyNotFoundException($"Grading system with ID {gradingSystemId} not found");
        }

        var strategy = _strategyFactory.GetStrategy(gradingSystem);
        return strategy.DetermineStatus(score, gradingSystem.MinimumPassingScore);
    }

    public decimal ConvertGradeToScore(string grade, Guid gradingSystemId)
    {
        var gradingSystem = _unitOfWork.GradingSystems.GetWithGradeScalesAsync(gradingSystemId).GetAwaiter().GetResult();
        if (gradingSystem == null)
        {
            throw new KeyNotFoundException($"Grading system with ID {gradingSystemId} not found");
        }

        var strategy = _strategyFactory.GetStrategy(gradingSystem);
        return strategy.ConvertGradeToScore(grade);
    }
}