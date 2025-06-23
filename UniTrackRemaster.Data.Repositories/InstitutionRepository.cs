using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Data.Repositories;

public class InstitutionRepository : Repository<Institution>, IInstitutionRepository
{
    private readonly UniTrackDbContext _context;

    public InstitutionRepository(UniTrackDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Institution?> GetByIdAsync(Guid id)
    {
        return await _context.Institutions
            .Include(e => e.Address)
            .Include(e => e.Images)
            .Include(e => e.Students)
            .Include(e => e.Teachers)
            .Include(e => e.Events)
            .FirstOrDefaultAsync(e => e.Id == id);
    }
public async Task<List<Institution>> GetInstitutionsByUserIdAsync(string userId)
{
    // First approach: Get institutions through joined user-institutions table
    // This is the most efficient if you're using the updated approach where users are directly linked to institutions
    var userInstitutions = await _context.Users
        .Where(u => u.Id.ToString() == userId)
        .SelectMany(u => u.Institutions)
        .Include(i => i.Address)
        .Include(i => i.Images)
        .ToListAsync();

    if (userInstitutions.Any())
    {
        return userInstitutions;
    }

    // Fallback approach: Get institutions through different user roles
    var institutionIds = new HashSet<Guid>();

    // Find institutions through Admin role
    var adminInstitutions = await _context.Admins
        .Where(a => a.UserId.ToString() == userId)
        .Select(a => a.InstitutionId)
        .ToListAsync();

    foreach (var id in adminInstitutions)
    {
        institutionIds.Add(id);
    }

    // Find institutions through Teacher role
    var teacherInstitutions = await _context.Teachers
        .Where(t => t.UserId.ToString() == userId)
        .Select(t => t.InstitutionId)
        .ToListAsync();

    foreach (var id in teacherInstitutions)
    {
        institutionIds.Add(id);
    }

    // Find institutions through Parent role (via their children's schools/universities)
    var parentStudentInfo = await _context.Parents
        .Where(p => p.UserId.ToString() == userId)
        .SelectMany(p => p.Children)
        .Where(student => student.SchoolId.HasValue || student.UniversityId.HasValue)
        .Select(student => new { student.SchoolId, student.UniversityId })
        .ToListAsync();

    // Extract school IDs and get their institution IDs
    var schoolIds = parentStudentInfo
        .Where(p => p.SchoolId.HasValue)
        .Select(p => p.SchoolId!.Value)
        .ToList();

    if (schoolIds.Any())
    {
        var schoolInstitutionIds = await _context.Schools
            .Where(s => schoolIds.Contains(s.Id))
            .Select(s => s.InstitutionId)
            .ToListAsync();

        foreach (var id in schoolInstitutionIds)
        {
            institutionIds.Add(id);
        }
    }

    // Extract university IDs and get their institution IDs
    var universityIds = parentStudentInfo
        .Where(p => p.UniversityId.HasValue)
        .Select(p => p.UniversityId!.Value)
        .ToList();

    if (universityIds.Any())
    {
        var universityInstitutionIds = await _context.Universities
            .Where(u => universityIds.Contains(u.Id))
            .Select(u => u.InstitutionId)
            .ToListAsync();

        foreach (var id in universityInstitutionIds)
        {
            institutionIds.Add(id);
        }
    }

    // Find institutions through School-Student relationship
    var schoolStudents = await _context.Students
        .Where(s => s.UserId.ToString() == userId && s.SchoolId.HasValue)
        .Join(_context.Schools,
            student => student.SchoolId,
            school => school.Id,
            (student, school) => school.InstitutionId)
        .ToListAsync();

    foreach (var id in schoolStudents)
    {
        institutionIds.Add(id);
    }

    // Find institutions through University-Student relationship
    var universityStudents = await _context.Students
        .Where(s => s.UserId.ToString() == userId && s.UniversityId.HasValue)
        .Join(_context.Universities,
            student => student.UniversityId,
            university => university.Id,
            (student, university) => university.InstitutionId)
        .ToListAsync();

    foreach (var id in universityStudents)
    {
        institutionIds.Add(id);
    }

    // Get the institutions with all the needed includes
    var institutions = await _context.Institutions
        .Where(i => institutionIds.Contains(i.Id))
        .Include(i => i.Address)
        .Include(i => i.Images)
        .ToListAsync();

    return institutions;
}
    public async Task<List<Institution>> GetAllAsync()
    {
        return await _context.Institutions
            .Include(e => e.Address)
            .Include(e => e.Images)
            .ToListAsync();
    }

    public async Task<List<Institution>> GetAllAsync(string? nameFilter = null, string? typeFilter = null, 
        string? locationFilter = null, string? integrationStatusFilter = null, string? accreditationsFilter = null, 
        int page = 1, int pageSize = 50)
    {
        var query = _context.Institutions
            .Include(i => i.Address)
            .Include(i => i.Images)
            .AsQueryable();

        // Apply name filter if provided (case-insensitive partial match)
        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            query = query.Where(i => i.Name.ToLower().Contains(nameFilter.ToLower()));
        }

        // Apply type filter if provided
        if (!string.IsNullOrWhiteSpace(typeFilter) && Enum.TryParse<InstitutionType>(typeFilter, true, out var type))
        {
            query = query.Where(i => i.Type == type);
        }

        // Apply location filter if provided
        if (!string.IsNullOrWhiteSpace(locationFilter) && Enum.TryParse<LocationType>(locationFilter, true, out var location))
        {
            query = query.Where(i => i.Location == location);
        }

        // Apply integration status filter if provided
        if (!string.IsNullOrWhiteSpace(integrationStatusFilter) && 
            Enum.TryParse<IntegrationStatus>(integrationStatusFilter, true, out var integrationStatus))
        {
            query = query.Where(i => i.IntegrationStatus == integrationStatus);
        }

        // Apply accreditations filter if provided (comma-separated list)
        if (!string.IsNullOrWhiteSpace(accreditationsFilter))
        {
            var accreditationStrings = accreditationsFilter.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim())
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .ToList();

            var accreditations = new List<AccreditationType>();
            foreach (var accreditationString in accreditationStrings)
            {
                if (Enum.TryParse<AccreditationType>(accreditationString, true, out var accreditation))
                {
                    accreditations.Add(accreditation);
                }
            }

            if (accreditations.Any())
            {
                // Filter institutions that have ANY of the specified accreditations
                query = query.Where(i => i.Accreditations.Any(a => accreditations.Contains(a)));
            }
        }

        // Apply pagination
        return await query
            .OrderBy(i => i.Name) // Default ordering by name
            .ThenBy(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(string? nameFilter = null, string? typeFilter = null, 
        string? locationFilter = null, string? integrationStatusFilter = null, string? accreditationsFilter = null)
    {
        var query = _context.Institutions.AsQueryable();

        // Apply the same filters as in GetAllAsync method
        
        // Apply name filter if provided (case-insensitive partial match)
        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            query = query.Where(i => i.Name.ToLower().Contains(nameFilter.ToLower()));
        }

        // Apply type filter if provided
        if (!string.IsNullOrWhiteSpace(typeFilter) && Enum.TryParse<InstitutionType>(typeFilter, true, out var type))
        {
            query = query.Where(i => i.Type == type);
        }

        // Apply location filter if provided
        if (!string.IsNullOrWhiteSpace(locationFilter) && Enum.TryParse<LocationType>(locationFilter, true, out var location))
        {
            query = query.Where(i => i.Location == location);
        }

        // Apply integration status filter if provided
        if (!string.IsNullOrWhiteSpace(integrationStatusFilter) && 
            Enum.TryParse<IntegrationStatus>(integrationStatusFilter, true, out var integrationStatus))
        {
            query = query.Where(i => i.IntegrationStatus == integrationStatus);
        }

        // Apply accreditations filter if provided (comma-separated list)
        if (!string.IsNullOrWhiteSpace(accreditationsFilter))
        {
            var accreditationStrings = accreditationsFilter.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim())
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .ToList();

            var accreditations = new List<AccreditationType>();
            foreach (var accreditationString in accreditationStrings)
            {
                if (Enum.TryParse<AccreditationType>(accreditationString, true, out var accreditation))
                {
                    accreditations.Add(accreditation);
                }
            }

            if (accreditations.Any())
            {
                // Filter institutions that have ANY of the specified accreditations
                query = query.Where(i => i.Accreditations.Any(a => accreditations.Contains(a)));
            }
        }

        return await query.CountAsync();
    }

    public async Task<Institution> AddAsync(Institution entity)
    {
        await _context.Institutions.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Institution entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Institutions.FindAsync(id);
        if (entity != null)
        {
            _context.Institutions.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Institutions.AnyAsync(e => e.Id == id);
    }
}