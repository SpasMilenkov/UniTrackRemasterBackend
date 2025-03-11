using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Api.Dto.Institution;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Data.Repositories;

public class SchoolRepository(UniTrackDbContext context) : ISchoolRepository
{
    public async Task<School> InitSchoolAsync(InitSchoolDto initDto)
    {
        // Load the educational institution
        var institution = await context.Institutions
            .FirstOrDefaultAsync(ei => ei.Id == initDto.Id);
        
        if (institution == null)
            throw new InstitutionNotFoundException(initDto.Id);
        var application = await context.Applications.FirstOrDefaultAsync(a => a.InstitutionId == initDto.Id);
        if (application == null)
            throw new InvalidOperationException("Application not found");

        if(application.Status != ApplicationStatus.Approved)
            throw new InvalidOperationException("Application is not approved");
        
        // Check if this institution is already associated with a school
        var existingSchool = await context.Schools
            .FirstOrDefaultAsync(s => s.InstitutionId == initDto.Id);
    
        if (existingSchool != null)
            throw new InstitutionAlreadyInitializedException(initDto.Id);

        // Create new School entity
        var school = new School
        {
            Id = Guid.NewGuid(),
            InstitutionId = institution.Id,
            Programs = initDto.Programs,
            ExtracurricularActivities = new List<string>()
        };

        // Update the institution
        institution.Type = Enum.Parse<InstitutionType>(initDto.Type);
        institution.Name = initDto.Name;
        institution.Description = initDto.Description;
        institution.Website = initDto.Website;
        institution.Motto = initDto.Motto;
        institution.EstablishedDate = initDto.EstablishedDate;
        institution.IntegrationStatus = IntegrationStatus.Active;
        
        application.Status = ApplicationStatus.Verified;
        await context.Schools.AddAsync(school);
        await context.SaveChangesAsync();
    
        return school;
    }
    public async Task<School> GetSchoolAsync(Guid schoolId)
    {
        var school = await context.Schools
            .Include(s => s.Institution)  // Include the related institution
            .ThenInclude(i => i.Images)
            .Include(s => s.Institution)
            .ThenInclude(i => i.Address)
            .Include(s => s.SchoolReport)
            .FirstOrDefaultAsync(s => s.Id == schoolId);
            
        if (school is null)
            throw new ArgumentException();
            
        return school;
    }

    public async Task<List<School>> GetSchoolsAsync(SchoolFilterDto filter)
    {
        var query = context.Schools
            .Include(s => s.Institution)
            .ThenInclude(i => i.Images)
            .Include(s => s.Institution)
            .ThenInclude(i => i.Address)
            .Where(s => s.Institution.IntegrationStatus == IntegrationStatus.Active);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.ToLower();
            query = query.Where(s =>
                s.Institution.Name.ToLower().Contains(searchTerm) ||
                s.Institution.Description.ToLower().Contains(searchTerm) ||
                s.Institution.Address.Country.ToLower().Contains(searchTerm) ||
                s.Institution.Address.Settlement.ToLower().Contains(searchTerm)
            );
        }

        // Apply type filters
        if (filter.Types != null && filter.Types.Any())
        {
            var institutionTypes = filter.Types
                .Select(t => Enum.Parse<InstitutionType>(t))
                .ToList();
            query = query.Where(s => institutionTypes.Contains(s.Institution.Type));
        }
        
        return await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();
    }

    public async Task<School> UpdateSchoolAsync(UpdateSchoolDto updateDto)
    {
        var school = await GetSchoolAsync(updateDto.SchoolId);
        var institution = school.Institution;

        // Update institution properties
        if (updateDto.Name is not null)
            institution.Name = updateDto.Name;
        if (updateDto.Description is not null)
            institution.Description = updateDto.Description;

        // Update school-specific properties
        // Add any school-specific updates here...

        await context.SaveChangesAsync();
        return school;
    }

    public async Task DeleteSchoolAsync(Guid schoolId)
    {
        var school = await GetSchoolAsync(schoolId);
        context.Schools.Remove(school);
        // Optionally, also delete the institution if that's the intended behavior
        if (school.Institution != null)
        {
            context.Institutions.Remove(school.Institution);
        }
        await context.SaveChangesAsync();
    }
}
public class InstitutionNotFoundException : Exception
{
    public InstitutionNotFoundException(Guid id) 
        : base($"Educational Institution with ID {id} was not found.")
    {
    }
}

public class InstitutionAlreadyInitializedException : Exception
{
    public InstitutionAlreadyInitializedException(Guid id) 
        : base($"Institution with ID {id} has already been initialized.")
    {
    }
}