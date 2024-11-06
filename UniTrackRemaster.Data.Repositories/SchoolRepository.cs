using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Events;
using UniTrackRemaster.Data.Models.Location;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Data.Repositories;

public class SchoolRepository(UniTrackDbContext context) : ISchoolRepository
{
    public async Task<School> CreateSchoolAsync(string schoolName, SchoolAddress address)
    {
        var school = new School
        {
            Address = address,
            Name = schoolName,
            IntegrationStatus = IntegrationStatus.RequiresVerification,
        };
        await context.Schools.AddAsync(school);
        await context.SaveChangesAsync();
        return school;
    }

    public async Task<School> InitSchoolAsync(InitSchoolDto initDto)
    {
        var school = context.Schools.FirstOrDefault(school => school.Id == initDto.Id);
        if (school == null) return null;

        if (initDto.Name != school.Name)
            school.Name = initDto.Name;
        
        school.IntegrationStatus = IntegrationStatus.Active;
        school.Description = initDto.Description;
        school.Motto = initDto.Motto;
        school.Programs = initDto.Programs;
        school.Website = initDto.Website;
        await context.SaveChangesAsync();
        return school;
    }

    public async Task<School> GetSchoolAsync(Guid schoolId)
    {
        var school = await context.Schools.FindAsync(schoolId);
        if (school is null)
            throw new ArgumentException();
        return school;
    }

    public async Task<List<School>> GetSchoolsAsync(int pageNumber, int pageSize)
    {
        return await context.Schools
            .Include(s => s.Images)
            .Where(S => S.IntegrationStatus == IntegrationStatus.Active)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<School> UpdateSchoolAsync(UpdateSchoolDto updateDto)
    {
        var school = await GetSchoolAsync(updateDto.SchoolId);

        if (updateDto.Name is not null)
            school.Name = updateDto.Name;
        if (updateDto.TeacherIds is not null)
        {
            var teacherIds = new HashSet<Guid>(updateDto.TeacherIds);
            school.Teachers = context.Teachers.Where(major => teacherIds.Contains(major.Id)).ToList();
        }
        if (updateDto.StudentIds is not null)
        {
            var studentIds = new HashSet<Guid>(updateDto.StudentIds);
            school.Students = context.Students.Where(major => studentIds.Contains(major.Id)).ToList();
        }

        await context.SaveChangesAsync();

        return school;
    }

    public async Task DeleteSchoolAsync(Guid schoolId)
    {
        var school = await GetSchoolAsync(schoolId);
        context.Entry(school).State = EntityState.Deleted;
        
    }
}