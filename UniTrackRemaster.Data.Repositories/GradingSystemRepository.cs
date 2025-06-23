using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Data.Repositories;

public class GradingSystemRepository : IGradingSystemRepository
{
    private readonly UniTrackDbContext _context;

    public GradingSystemRepository(UniTrackDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets a grading system by ID
    /// </summary>
    public async Task<GradingSystem> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.GradingSystems
            .FirstOrDefaultAsync(gs => gs.Id == id, cancellationToken);
    }

    /// <summary>
    /// Gets all grading systems
    /// </summary>
    public async Task<List<GradingSystem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.GradingSystems
            .Include(gs => gs.GradeScales)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the default grading system for an institution
    /// </summary>
    public async Task<GradingSystem> GetDefaultForInstitutionAsync(Guid institutionId, CancellationToken cancellationToken = default)
    {
        return await _context.GradingSystems
            .Include(gs => gs.GradeScales)
            .Where(gs => gs.InstitutionId == institutionId && gs.IsDefault)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all grading systems for an institution
    /// </summary>
    public async Task<List<GradingSystem>> GetAllForInstitutionAsync(Guid institutionId, CancellationToken cancellationToken = default)
    {
        return await _context.GradingSystems
            .Include(gs => gs.GradeScales)
            .Where(gs => gs.InstitutionId == institutionId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a grading system with its grade scales included
    /// </summary>
    public async Task<GradingSystem> GetWithGradeScalesAsync(Guid gradingSystemId, CancellationToken cancellationToken = default)
    {
        return await _context.GradingSystems
            .Include(gs => gs.GradeScales)
            .FirstOrDefaultAsync(gs => gs.Id == gradingSystemId, cancellationToken);
    }

    /// <summary>
    /// Sets a grading system as the default for an institution
    /// </summary>
    public async Task<bool> SetDefaultAsync(Guid gradingSystemId, Guid institutionId, CancellationToken cancellationToken = default)
    {
        // Remove default from all other grading systems for this institution
        var existingSystems = await _context.GradingSystems
            .Where(gs => gs.InstitutionId == institutionId && gs.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var system in existingSystems)
        {
            system.IsDefault = false;
            _context.GradingSystems.Update(system);
        }

        // Set the new default
        var newDefaultSystem = await _context.GradingSystems
            .FirstOrDefaultAsync(gs => gs.Id == gradingSystemId && gs.InstitutionId == institutionId, cancellationToken);

        if (newDefaultSystem == null)
        {
            return false;
        }

        newDefaultSystem.IsDefault = true;
        _context.GradingSystems.Update(newDefaultSystem);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// Adds a new grading system
    /// </summary>
    public async Task<GradingSystem> AddAsync(GradingSystem gradingSystem, CancellationToken cancellationToken = default)
    {
        await _context.GradingSystems.AddAsync(gradingSystem, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return gradingSystem;
    }

    /// <summary>
    /// Updates an existing grading system
    /// </summary>
    public async Task<GradingSystem> UpdateAsync(GradingSystem gradingSystem, CancellationToken cancellationToken = default)
    {
        // First, find the existing entity to update
        var existingSystem = await _context.GradingSystems
            .Include(gs => gs.GradeScales)
            .FirstOrDefaultAsync(gs => gs.Id == gradingSystem.Id, cancellationToken);

        if (existingSystem == null)
        {
            throw new KeyNotFoundException($"Grading system with ID {gradingSystem.Id} not found");
        }

        // Update base properties
        existingSystem.Name = gradingSystem.Name;
        existingSystem.Description = gradingSystem.Description;
        existingSystem.IsDefault = gradingSystem.IsDefault;
        existingSystem.MinimumPassingScore = gradingSystem.MinimumPassingScore;
        existingSystem.MaximumScore = gradingSystem.MaximumScore;

        // Update grade scales
        if (gradingSystem.GradeScales != null)
        {
            // Remove existing scales
            _context.GradeScales.RemoveRange(existingSystem.GradeScales);

            // Add updated scales
            foreach (var scale in gradingSystem.GradeScales)
            {
                scale.GradingSystemId = existingSystem.Id;
                await _context.GradeScales.AddAsync(scale, cancellationToken);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return await GetWithGradeScalesAsync(gradingSystem.Id, cancellationToken);
    }

    /// <summary>
    /// Deletes a grading system
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var gradingSystem = await _context.GradingSystems
            .Include(gs => gs.GradeScales)
            .FirstOrDefaultAsync(gs => gs.Id == id, cancellationToken);

        if (gradingSystem == null)
        {
            return false;
        }

        // Prevent deletion of default grading system
        if (gradingSystem.IsDefault)
        {
            throw new InvalidOperationException("Cannot delete the default grading system");
        }

        // Remove all grade scales first
        _context.GradeScales.RemoveRange(gradingSystem.GradeScales);

        // Then remove the grading system
        _context.GradingSystems.Remove(gradingSystem);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
    public async Task<GradingSystem> GetByNameAndInstitutionAsync(string name, Guid institutionId, CancellationToken cancellationToken = default)
    {
        return await _context.GradingSystems
            .FirstOrDefaultAsync(gs =>
                gs.Name.ToLower() == name.ToLower() &&
                gs.InstitutionId == institutionId,
                cancellationToken);
    }
}
