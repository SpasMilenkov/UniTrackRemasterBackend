using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Api.Dto.Event;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Events;
namespace UniTrackRemaster.Data.Repositories;

public class EventRepository : IEventRepository
{
    private readonly UniTrackDbContext _context;

    public EventRepository(UniTrackDbContext context)
    {
        _context = context;
    }

    public async Task<Event?> GetByIdAsync(Guid id, bool includeRelated = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Events.AsQueryable();

        if (includeRelated)
        {
            query = query
                .Include(e => e.Organizer)
                    .ThenInclude(o => o.User)
                .Include(e => e.Institution)
                .Include(e => e.Participants)
                    .ThenInclude(p => p.User)
                .Include(e => e.Attenders)
                    .ThenInclude(a => a.User)
                .Include(e => e.Notifications)
                    .ThenInclude(n => n.User);
        }

        return await query.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Include(e => e.Organizer)
                .ThenInclude(o => o.User)
            .Include(e => e.Institution)
            .OrderByDescending(e => e.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Event>> GetByOrganizerAsync(Guid organizerId, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Include(e => e.Organizer)
                .ThenInclude(o => o.User)
            .Include(e => e.Institution)
            .Where(e => e.OrganizerId == organizerId)
            .OrderByDescending(e => e.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Event>> GetByInstitutionAsync(Guid institutionId, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Include(e => e.Organizer)
                .ThenInclude(o => o.User)
            .Include(e => e.Institution)
            .Where(e => e.InstitutionId == institutionId)
            .OrderByDescending(e => e.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Event>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Include(e => e.Organizer)
                .ThenInclude(o => o.User)
            .Include(e => e.Institution)
            .Where(e => e.StartDate >= startDate && e.EndDate <= endDate)
            .OrderBy(e => e.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Event>> GetByParticipantAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Include(e => e.Organizer)
                .ThenInclude(o => o.User)
            .Include(e => e.Institution)
            .Include(e => e.Participants)
            .Where(e => e.Participants.Any(p => p.UserId == userId))
            .OrderByDescending(e => e.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Event>> GetUpcomingEventsAsync(Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Events
            .Include(e => e.Organizer)
                .ThenInclude(o => o.User)
            .Include(e => e.Institution)
            .Where(e => e.StartDate > DateTime.UtcNow && e.Status == EventStatus.Scheduled);

        if (userId.HasValue)
        {
            query = query.Where(e => e.Participants.Any(p => p.UserId == userId.Value) ||
                                   e.OrganizerId == userId.Value);
        }

        return await query
            .OrderBy(e => e.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Event>> GetFilteredAsync(EventFilterParams filterParams, CancellationToken cancellationToken = default)
    {
        var query = _context.Events
            .Include(e => e.Organizer)
                .ThenInclude(o => o.User)
            .Include(e => e.Institution)
            .AsQueryable();

        if (filterParams.StartDate.HasValue)
            query = query.Where(e => e.StartDate >= filterParams.StartDate.Value);

        if (filterParams.EndDate.HasValue)
            query = query.Where(e => e.EndDate <= filterParams.EndDate.Value);

        if (filterParams.Type.HasValue)
            query = query.Where(e => e.Type == filterParams.Type.Value);

        if (filterParams.Status.HasValue)
            query = query.Where(e => e.Status == filterParams.Status.Value);

        if (filterParams.OrganizerId.HasValue)
            query = query.Where(e => e.OrganizerId == filterParams.OrganizerId.Value);

        if (filterParams.InstitutionId.HasValue)
            query = query.Where(e => e.InstitutionId == filterParams.InstitutionId.Value);

        if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
        {
            var searchTerm = filterParams.SearchTerm.ToLower();
            query = query.Where(e =>
                e.Title.ToLower().Contains(searchTerm) ||
                e.Topic.ToLower().Contains(searchTerm) ||
                (e.Description != null && e.Description.ToLower().Contains(searchTerm)));
        }

        return await query
            .OrderByDescending(e => e.StartDate)
            .Skip((filterParams.Page - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<Event> CreateAsync(Event eventEntity, CancellationToken cancellationToken = default)
    {
        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync(cancellationToken);
        return eventEntity;
    }

    public async Task<Event> UpdateAsync(Event eventEntity, CancellationToken cancellationToken = default)
    {
        eventEntity.UpdatedAt = DateTime.UtcNow;
        _context.Events.Update(eventEntity);
        await _context.SaveChangesAsync(cancellationToken);
        return eventEntity;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var eventEntity = await _context.Events.FindAsync(new object[] { id }, cancellationToken);
        if (eventEntity != null)
        {
            _context.Events.Remove(eventEntity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Events.AnyAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Events.CountAsync(cancellationToken);
    }

    public async Task<Dictionary<EventStatus, int>> GetEventCountsByStatusAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .GroupBy(e => e.Status)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }

    public async Task<Dictionary<EventType, int>> GetEventCountsByTypeAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .GroupBy(e => e.Type)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }
}