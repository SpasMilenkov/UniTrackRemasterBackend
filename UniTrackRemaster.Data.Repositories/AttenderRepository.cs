using System;
using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Events;

namespace UniTrackRemaster.Data.Repositories;

public class AttenderRepository : IAttenderRepository
{
    private readonly UniTrackDbContext _context;

    public AttenderRepository(UniTrackDbContext context)
    {
        _context = context;
    }

    public async Task<Attender?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Attenders
            .Include(a => a.User)
            .Include(a => a.Events)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Attender>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.Attenders
            .Include(a => a.User)
            .Where(a => a.Events.Any(e => e.Id == eventId))
            .OrderBy(a => a.User.FirstName)
            .ThenBy(a => a.User.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Attender>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Attenders
            .Include(a => a.Events)
                .ThenInclude(e => e.Organizer)
                    .ThenInclude(o => o.User)
            .Include(a => a.Events)
                .ThenInclude(e => e.Institution)
            .Where(a => a.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Attender?> GetByEventAndUserAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Attenders
            .Include(a => a.User)
            .Include(a => a.Events)
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Events.Any(e => e.Id == eventId), cancellationToken);
    }

    public async Task<Attender> CreateAsync(Attender attender, CancellationToken cancellationToken = default)
    {
        _context.Attenders.Add(attender);
        await _context.SaveChangesAsync(cancellationToken);
        return attender;
    }

    public async Task<Attender> UpdateAsync(Attender attender, CancellationToken cancellationToken = default)
    {
        attender.UpdatedAt = DateTime.UtcNow;
        _context.Attenders.Update(attender);
        await _context.SaveChangesAsync(cancellationToken);
        return attender;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var attender = await _context.Attenders.FindAsync(new object[] { id }, cancellationToken);
        if (attender != null)
        {
            _context.Attenders.Remove(attender);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<Dictionary<AttendanceStatus, int>> GetAttendanceCountsByStatusAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.Attenders
            .Where(a => a.Events.Any(e => e.Id == eventId))
            .GroupBy(a => a.AttendanceStatus)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }
}
