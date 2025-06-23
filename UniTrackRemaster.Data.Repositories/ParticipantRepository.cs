using System;
using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Events;

namespace UniTrackRemaster.Data.Repositories;

public class ParticipantRepository : IParticipantRepository
{
    private readonly UniTrackDbContext _context;

    public ParticipantRepository(UniTrackDbContext context)
    {
        _context = context;
    }

    public async Task<Participant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Participants
            .Include(p => p.User)
            .Include(p => p.Event)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Participant>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.Participants
            .Include(p => p.User)
            .Where(p => p.EventId == eventId)
            .OrderBy(p => p.User.FirstName)
            .ThenBy(p => p.User.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Participant>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Participants
            .Include(p => p.Event)
                .ThenInclude(e => e.Organizer)
                    .ThenInclude(o => o.User)
            .Include(p => p.Event)
                .ThenInclude(e => e.Institution)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.Event.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Participant?> GetByEventAndUserAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Participants
            .Include(p => p.User)
            .Include(p => p.Event)
            .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId, cancellationToken);
    }

    public async Task<Participant> CreateAsync(Participant participant, CancellationToken cancellationToken = default)
    {
        _context.Participants.Add(participant);
        await _context.SaveChangesAsync(cancellationToken);
        return participant;
    }

    public async Task<Participant> UpdateAsync(Participant participant, CancellationToken cancellationToken = default)
    {
        participant.UpdatedAt = DateTime.UtcNow;
        _context.Participants.Update(participant);
        await _context.SaveChangesAsync(cancellationToken);
        return participant;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var participant = await _context.Participants.FindAsync(new object[] { id }, cancellationToken);
        if (participant != null)
        {
            _context.Participants.Remove(participant);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeleteByEventAndUserAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
    {
        var participant = await _context.Participants
            .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId, cancellationToken);

        if (participant != null)
        {
            _context.Participants.Remove(participant);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> IsUserParticipantAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Participants
            .AnyAsync(p => p.EventId == eventId && p.UserId == userId, cancellationToken);
    }

    public async Task<Dictionary<ParticipantStatus, int>> GetParticipantCountsByStatusAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.Participants
            .Where(p => p.EventId == eventId)
            .GroupBy(p => p.Status)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }
}