using System;
using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Events;

namespace UniTrackRemaster.Data.Repositories;

public class EventNotificationRepository : IEventNotificationRepository
{
    private readonly UniTrackDbContext _context;

    public EventNotificationRepository(UniTrackDbContext context)
    {
        _context = context;
    }

    public async Task<EventNotification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.EventNotifications
            .Include(n => n.Event)
            .Include(n => n.User)
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<EventNotification>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.EventNotifications
            .Include(n => n.User)
            .Where(n => n.EventId == eventId)
            .OrderBy(n => n.SendAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EventNotification>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.EventNotifications
            .Include(n => n.Event)
                .ThenInclude(e => e.Organizer)
                    .ThenInclude(o => o.User)
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.SendAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EventNotification>> GetPendingNotificationsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EventNotifications
            .Include(n => n.Event)
            .Include(n => n.User)
            .Where(n => !n.IsSent && n.SendAt <= DateTime.UtcNow)
            .OrderBy(n => n.SendAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<EventNotification> CreateAsync(EventNotification notification, CancellationToken cancellationToken = default)
    {
        _context.EventNotifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);
        return notification;
    }

    public async Task<EventNotification> UpdateAsync(EventNotification notification, CancellationToken cancellationToken = default)
    {
        notification.UpdatedAt = DateTime.UtcNow;
        _context.EventNotifications.Update(notification);
        await _context.SaveChangesAsync(cancellationToken);
        return notification;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var notification = await _context.EventNotifications.FindAsync(new object[] { id }, cancellationToken);
        if (notification != null)
        {
            _context.EventNotifications.Remove(notification);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAsSentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var notification = await _context.EventNotifications.FindAsync(new object[] { id }, cancellationToken);
        if (notification != null)
        {
            notification.IsSent = true;
            notification.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<EventNotification>> CreateBulkAsync(IEnumerable<EventNotification> notifications, CancellationToken cancellationToken = default)
    {
        _context.EventNotifications.AddRange(notifications);
        await _context.SaveChangesAsync(cancellationToken);
        return notifications;
    }
}