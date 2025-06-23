using System;
using UniTrackRemaster.Api.Dto.Event;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Events;

namespace UniTrackRemaster.Services.Organization;

public class EventService : IEventService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventNotificationService _notificationService;

    public EventService(IUnitOfWork unitOfWork, IEventNotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<EventResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var eventEntity = await _unitOfWork.Events.GetByIdAsync(id, false, cancellationToken);
        if (eventEntity == null)
            throw new KeyNotFoundException($"Event with ID {id} not found.");

        return EventResponseDto.FromEntity(eventEntity);
    }

    public async Task<EventDetailResponseDto> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var eventEntity = await _unitOfWork.Events.GetByIdAsync(id, true, cancellationToken);
        if (eventEntity == null)
            throw new KeyNotFoundException($"Event with ID {id} not found.");

        return EventDetailResponseDto.FromEntity(eventEntity);
    }

    public async Task<IEnumerable<EventResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var events = await _unitOfWork.Events.GetAllAsync(cancellationToken);
        return events.Select(EventResponseDto.FromEntity);
    }

    public async Task<IEnumerable<EventResponseDto>> GetByOrganizerAsync(Guid organizerId, CancellationToken cancellationToken = default)
    {
        var events = await _unitOfWork.Events.GetByOrganizerAsync(organizerId, cancellationToken);
        return events.Select(EventResponseDto.FromEntity);
    }

    public async Task<IEnumerable<EventResponseDto>> GetByInstitutionAsync(Guid institutionId, CancellationToken cancellationToken = default)
    {
        var events = await _unitOfWork.Events.GetByInstitutionAsync(institutionId, cancellationToken);
        return events.Select(EventResponseDto.FromEntity);
    }

    public async Task<IEnumerable<EventResponseDto>> GetByParticipantAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var events = await _unitOfWork.Events.GetByParticipantAsync(userId, cancellationToken);
        return events.Select(EventResponseDto.FromEntity);
    }

    public async Task<IEnumerable<EventResponseDto>> GetUpcomingEventsAsync(Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var events = await _unitOfWork.Events.GetUpcomingEventsAsync(userId, cancellationToken);
        return events.Select(EventResponseDto.FromEntity);
    }

    public async Task<IEnumerable<EventResponseDto>> GetFilteredAsync(EventFilterDto filterDto, CancellationToken cancellationToken = default)
    {
        var filterParams = new EventFilterParams
        {
            StartDate = filterDto.StartDate,
            EndDate = filterDto.EndDate,
            Type = filterDto.Type,
            Status = filterDto.Status,
            OrganizerId = filterDto.OrganizerId,
            InstitutionId = filterDto.InstitutionId,
            SearchTerm = filterDto.SearchTerm,
            IsPublic = filterDto.IsPublic,
            Page = filterDto.Page,
            PageSize = filterDto.PageSize
        };

        var events = await _unitOfWork.Events.GetFilteredAsync(filterParams, cancellationToken);
        return events.Select(EventResponseDto.FromEntity);
    }

    public async Task<EventResponseDto> CreateAsync(CreateEventDto createDto, Guid organizerUserId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Get or create organizer
            var organizer = await _unitOfWork.Organizers.GetByUserIdAsync(organizerUserId, cancellationToken);
            if (organizer == null)
            {
                throw new InvalidOperationException($"User {organizerUserId} is not registered as an organizer.");
            }

            // Create event entity
            var eventEntity = createDto.ToEntity(organizer.Id);
            var createdEvent = await _unitOfWork.Events.CreateAsync(eventEntity, cancellationToken);

            // Add participants if specified
            if (createDto.ParticipantUserIds?.Any() == true)
            {
                await AddParticipantsAsync(createdEvent.Id, createDto.ParticipantUserIds, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Schedule notifications
            await _notificationService.ScheduleEventNotificationsAsync(createdEvent.Id, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);
            return EventResponseDto.FromEntity(createdEvent);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<EventResponseDto> UpdateAsync(Guid id, UpdateEventDto updateDto, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var existingEvent = await _unitOfWork.Events.GetByIdAsync(id, false, cancellationToken);
            if (existingEvent == null)
                throw new KeyNotFoundException($"Event with ID {id} not found.");

            // Apply updates
            updateDto.ApplyToEntity(existingEvent);
            var updatedEvent = await _unitOfWork.Events.UpdateAsync(existingEvent, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Notify participants of changes
            await _notificationService.NotifyEventUpdateAsync(id, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);
            return EventResponseDto.FromEntity(updatedEvent);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var existingEvent = await _unitOfWork.Events.GetByIdAsync(id, false, cancellationToken);
            if (existingEvent == null)
                throw new KeyNotFoundException($"Event with ID {id} not found.");

            // Notify participants of cancellation
            await _notificationService.NotifyEventCancellationAsync(id, cancellationToken);

            await _unitOfWork.Events.DeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<EventStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var totalEvents = await _unitOfWork.Events.GetTotalCountAsync(cancellationToken);
        var eventsByStatus = await _unitOfWork.Events.GetEventCountsByStatusAsync(cancellationToken);
        var eventsByType = await _unitOfWork.Events.GetEventCountsByTypeAsync(cancellationToken);

        return new EventStatsDto(
            TotalEvents: totalEvents,
            UpcomingEvents: eventsByStatus.GetValueOrDefault(EventStatus.Scheduled, 0),
            OngoingEvents: eventsByStatus.GetValueOrDefault(EventStatus.InProgress, 0),
            CompletedEvents: eventsByStatus.GetValueOrDefault(EventStatus.Completed, 0),
            CancelledEvents: eventsByStatus.GetValueOrDefault(EventStatus.Cancelled, 0),
            EventsByType: eventsByType,
            EventsByMonth: await GetEventsByMonthAsync(cancellationToken)
        );
    }

    public async Task<bool> CanUserModifyEventAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
    {
        var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId, true, cancellationToken);
        if (eventEntity == null)
            return false;

        // Check if user is the organizer
        return eventEntity.Organizer?.UserId == userId;
    }

    private async Task AddParticipantsAsync(Guid eventId, IEnumerable<Guid> userIds, CancellationToken cancellationToken)
    {
        foreach (var userId in userIds)
        {
            var createDto = new CreateParticipantDto(eventId, userId, false);
            var participant = createDto.ToEntity();
            await _unitOfWork.Participants.CreateAsync(participant, cancellationToken);
        }
    }

    private async Task<Dictionary<string, int>> GetEventsByMonthAsync(CancellationToken cancellationToken)
    {
        var currentDate = DateTime.UtcNow;
        var startDate = currentDate.AddMonths(-11).Date; // Last 12 months
        var events = await _unitOfWork.Events.GetByDateRangeAsync(startDate, currentDate, cancellationToken);

        return events
            .GroupBy(e => e.StartDate.ToString("yyyy-MM"))
            .ToDictionary(g => g.Key, g => g.Count());
    }
}
