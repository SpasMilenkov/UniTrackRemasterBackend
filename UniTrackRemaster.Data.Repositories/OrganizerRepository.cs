using System;
using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Events;

namespace UniTrackRemaster.Data.Repositories;

public class OrganizerRepository : IOrganizerRepository
{
    private readonly UniTrackDbContext _context;

    public OrganizerRepository(UniTrackDbContext context)
    {
        _context = context;
    }

    public async Task<Organizer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Organizers
            .Include(o => o.User)
            .Include(o => o.Institution)
            .Include(o => o.OrganizedEvents)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Organizer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Organizers
            .Include(o => o.User)
            .Include(o => o.Institution)
            .Include(o => o.OrganizedEvents)
            .FirstOrDefaultAsync(o => o.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<Organizer>> GetByInstitutionAsync(Guid institutionId, CancellationToken cancellationToken = default)
    {
        return await _context.Organizers
            .Include(o => o.User)
            .Include(o => o.Institution)
            .Where(o => o.InstitutionId == institutionId)
            .OrderBy(o => o.User.FirstName)
            .ThenBy(o => o.User.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Organizer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Organizers
            .Include(o => o.User)
            .Include(o => o.Institution)
            .OrderBy(o => o.User.FirstName)
            .ThenBy(o => o.User.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Organizer> CreateAsync(Organizer organizer, CancellationToken cancellationToken = default)
    {
        _context.Organizers.Add(organizer);
        await _context.SaveChangesAsync(cancellationToken);
        return organizer;
    }

    public async Task<Organizer> UpdateAsync(Organizer organizer, CancellationToken cancellationToken = default)
    {
        organizer.UpdatedAt = DateTime.UtcNow;
        _context.Organizers.Update(organizer);
        await _context.SaveChangesAsync(cancellationToken);
        return organizer;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var organizer = await _context.Organizers.FindAsync(new object[] { id }, cancellationToken);
        if (organizer != null)
        {
            _context.Organizers.Remove(organizer);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> IsUserOrganizerAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Organizers
            .AnyAsync(o => o.UserId == userId, cancellationToken);
    }

    public async Task<bool> CanCreatePublicEventsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var organizer = await _context.Organizers
            .FirstOrDefaultAsync(o => o.UserId == userId, cancellationToken);

        return organizer?.CanCreatePublicEvents ?? false;
    }
}