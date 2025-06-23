using System;
using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Event;

public record CreateEventDto(
    [Required][StringLength(255)] string Title,
    [Required][StringLength(255)] string Topic,
    [StringLength(500)] string? Description,
    [Required] DateTime StartDate,
    [Required] DateTime EndDate,
    string? Location,
    [Required] EventType Type,
    EventStatus Status,
    bool IsAllDay,
    bool IsRecurring,
    RecurrencePattern? RecurrencePattern,
    int? MaxParticipants,
    bool RequiresApproval,
    string? MeetingLink,
    string? Notes,
    Guid? InstitutionId,
    List<Guid>? ParticipantUserIds)
{
    public Data.Models.Events.Event ToEntity(Guid organizerId) => new()
    {
        Id = Guid.NewGuid(),
        Title = Title,
        Topic = Topic,
        Description = Description,
        StartDate = StartDate,
        EndDate = EndDate,
        Location = Location,
        Type = Type,
        Status = Status,
        IsAllDay = IsAllDay,
        IsRecurring = IsRecurring,
        RecurrencePattern = RecurrencePattern,
        MaxParticipants = MaxParticipants,
        RequiresApproval = RequiresApproval,
        MeetingLink = MeetingLink,
        Notes = Notes,
        OrganizerId = organizerId,
        InstitutionId = InstitutionId,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}