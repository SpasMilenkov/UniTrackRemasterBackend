using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Api.Dto.Institution;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Analytics;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Data.Repositories;

public class UniversityRepository(UniTrackDbContext context) : Repository<University>(context), IUniversityRepository
{
  
    public async Task<University> InitUniversityAsync(InitUniversityDto initDto)
    {
        // Load the educational institution
        var institution = await _context.Institutions
            .FirstOrDefaultAsync(ei => ei.Id == initDto.Id);

        if (institution == null)
            throw new InstitutionNotFoundException(initDto.Id);
        
        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.InstitutionId == initDto.Id);
        if (application == null)
            throw new InvalidOperationException("Application not found");

        if(application.Status != ApplicationStatus.Approved)
            throw new InvalidOperationException("Application is not approved");
        
        // Check if this institution is already associated with a university
        var existingUniversity = await _context.Universities
            .FirstOrDefaultAsync(u => u.InstitutionId == initDto.Id);

        if (existingUniversity != null)
            throw new InstitutionAlreadyInitializedException(initDto.Id);

        // Create new University entity
        var university = new University
        {
            Id = Guid.NewGuid(),
            InstitutionId = institution.Id,
            FocusAreas = initDto.FocusAreas.ToList(),
            UndergraduateCount = initDto.UndergraduateCount,
            GraduateCount = initDto.GraduateCount,
            AcceptanceRate = initDto.AcceptanceRate,
            ResearchFunding = initDto.ResearchFunding,
            HasStudentHousing = initDto.HasStudentHousing,
            Departments = initDto.Departments.ToList(),
        };

        // Update the institution
        institution.Type = InstitutionType.PublicUniversity;
        institution.Name = initDto.Name;
        institution.Description = initDto.Description;
        institution.Website = initDto.Website;
        institution.Motto = initDto.Motto;
        institution.EstablishedDate = initDto.EstablishedDate;
        institution.IntegrationStatus = IntegrationStatus.Active;
        institution.Type = initDto.Type;
        application.Status = ApplicationStatus.Verified;

        await _context.Universities.AddAsync(university);
        await _context.SaveChangesAsync();

        return university;
    }

    // public async Task<University?> GetByIdAsync(Guid universityId)
    // {
    //     return await _context.FindAsync<University>(universityId);
    // }

    // public async Task<University?> GetByInstitutionIdAsync(Guid institutionId)
    // {
    //     return await _context.Universities.FirstOrDefaultAsync(u => u.InstitutionId == institutionId);
    // }
    
        /// <inheritdoc />
    public async Task<IEnumerable<University>> GetAllAsync(string? searchQuery = null)
    {
        var query = _context.Universities
            .Include(u => u.Institution)
            .Include(u => u.Faculties)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            searchQuery = searchQuery.ToLower();
            query = query.Where(u => 
                (u.Institution.Name != null && u.Institution.Name.ToLower().Contains(searchQuery)) ||
                (u.Institution.Description != null && u.Institution.Description.ToLower().Contains(searchQuery)) ||
                (u.Institution.Motto != null && u.Institution.Motto.ToLower().Contains(searchQuery)) ||
                (u.Departments != null && u.Departments.Any(d => d.ToLower().Contains(searchQuery)))
            );
        }

        return await query.ToListAsync();
    }

    /// <inheritdoc />
    public async Task<University?> GetByIdAsync(Guid id)
    {
        return await _context.Universities
            .Include(u => u.Institution)
            .Include(u => u.Faculties)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    /// <inheritdoc />
    public async Task<University?> GetByInstitutionIdAsync(Guid institutionId)
    {
        return await _context.Universities
            .Include(u => u.Institution)
            .Include(u => u.Faculties)
            .FirstOrDefaultAsync(u => u.InstitutionId == institutionId);
    }

    /// <inheritdoc />
    public async Task<University> CreateAsync(University university)
    {
        _context.Universities.Add(university);
        await _context.SaveChangesAsync();
        return university;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(University university)
    {
        _context.Universities.Update(university);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id)
    {
        var university = await GetByIdAsync(id);
        if (university != null)
        {
            _context.Universities.Remove(university);
            await _context.SaveChangesAsync();
        }
    }
}

