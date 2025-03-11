using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Api.Dto.Institution;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Analytics;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Events;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Data.Repositories;

public class UniversityRepository : IUniversityRepository
{
    private readonly UniTrackDbContext context;

    public UniversityRepository(UniTrackDbContext context)
    {
        this.context = context;
    }

    public async Task<University> InitUniversityAsync(InitUniversityDto initDto)
    {
        // Load the educational institution
        var institution = await context.Institutions
            .FirstOrDefaultAsync(ei => ei.Id == initDto.Id);

        if (institution == null)
            throw new InstitutionNotFoundException(initDto.Id);
        
        var application = await context.Applications
            .FirstOrDefaultAsync(a => a.InstitutionId == initDto.Id);
        if (application == null)
            throw new InvalidOperationException("Application not found");

        if(application.Status != ApplicationStatus.Approved)
            throw new InvalidOperationException("Application is not approved");
        
        // Check if this institution is already associated with a university
        var existingUniversity = await context.Universities
            .FirstOrDefaultAsync(u => u.InstitutionId == initDto.Id);

        if (existingUniversity != null)
            throw new InstitutionAlreadyInitializedException(initDto.Id);

        // Create new UniversityReport
        var universityReport = new UniversityReport
        {
            Id = Guid.NewGuid(),
            Title = "Initial Report",
            From = DateTime.UtcNow,
            To = DateTime.UtcNow.AddYears(1),
            ShortDescription = "",
            DetailedDescription = "",
            NumericalRating = 0
        };

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
            UniversityReportId = universityReport.Id,
            UniversityReport = universityReport
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

        await context.UniversityReports.AddAsync(universityReport);
        await context.Universities.AddAsync(university);
        await context.SaveChangesAsync();

        return university;
    }
}

