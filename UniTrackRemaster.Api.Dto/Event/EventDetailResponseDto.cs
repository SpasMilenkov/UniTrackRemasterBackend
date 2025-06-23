using System;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Event;

public record EventDetailResponseDto(
    Guid Id,
    string Title,
    string Topic,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    string? Location,
    EventType Type,
    EventStatus Status,
    bool IsAllDay,
    bool IsRecurring,
    RecurrencePattern? RecurrencePattern,
    int? MaxParticipants,
    bool RequiresApproval,
    string? MeetingLink,
    string? Notes,
    Guid OrganizerId,
    string? OrganizerName,
    Guid? InstitutionId,
    string? InstitutionName,
    int ParticipantCount,
    int AttendeeCount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<ParticipantResponseDto> Participants,
    List<AttenderResponseDto> Attenders,
    List<EventNotificationResponseDto> Notifications)
{
    public static EventDetailResponseDto FromEntity(Data.Models.Events.Event eventEntity) => new(
        eventEntity.Id,
        eventEntity.Title,
        eventEntity.Topic,
        eventEntity.Description,
        eventEntity.StartDate,
        eventEntity.EndDate,
        eventEntity.Location,
        eventEntity.Type,
        eventEntity.Status,
        eventEntity.IsAllDay,
        eventEntity.IsRecurring,
        eventEntity.RecurrencePattern,
        eventEntity.MaxParticipants,
        eventEntity.RequiresApproval,
        eventEntity.MeetingLink,
        eventEntity.Notes,
        eventEntity.OrganizerId,
        eventEntity.Organizer?.User != null
            ? $"{eventEntity.Organizer.User.FirstName} {eventEntity.Organizer.User.LastName}"
            : null,
        eventEntity.InstitutionId,
        eventEntity.Institution?.Name,
        eventEntity.Participants?.Count ?? 0,
        eventEntity.Attenders?.Count ?? 0,
        eventEntity.CreatedAt,
        eventEntity.UpdatedAt,
        eventEntity.Participants?.Select(ParticipantResponseDto.FromEntity).ToList() ?? new(),
        eventEntity.Attenders?.Select(AttenderResponseDto.FromEntity).ToList() ?? new(),
        eventEntity.Notifications?.Select(EventNotificationResponseDto.FromEntity).ToList() ?? new()
    );
}