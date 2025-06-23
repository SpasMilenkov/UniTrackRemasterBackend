using System;
using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Event;

public record UpdateEventDto(
    [StringLength(255)] string? Title,
    [StringLength(255)] string? Topic,
    [StringLength(500)] string? Description,
    DateTime? StartDate,
    DateTime? EndDate,
    string? Location,
    EventType? Type,
    EventStatus? Status,
    bool? IsAllDay,
    bool? IsRecurring,
    RecurrencePattern? RecurrencePattern,
    int? MaxParticipants,
    bool? RequiresApproval,
    string? MeetingLink,
    string? Notes)
{
    public void ApplyToEntity(Data.Models.Events.Event eventEntity)
    {
        if (Title != null) eventEntity.Title = Title;
        if (Topic != null) eventEntity.Topic = Topic;
        if (Description != null) eventEntity.Description = Description;
        if (StartDate.HasValue) eventEntity.StartDate = StartDate.Value;
        if (EndDate.HasValue) eventEntity.EndDate = EndDate.Value;
        if (Location != null) eventEntity.Location = Location;
        if (Type.HasValue) eventEntity.Type = Type.Value;
        if (Status.HasValue) eventEntity.Status = Status.Value;
        if (IsAllDay.HasValue) eventEntity.IsAllDay = IsAllDay.Value;
        if (IsRecurring.HasValue) eventEntity.IsRecurring = IsRecurring.Value;
        if (RecurrencePattern.HasValue) eventEntity.RecurrencePattern = RecurrencePattern.Value;
        if (MaxParticipants.HasValue) eventEntity.MaxParticipants = MaxParticipants.Value;
        if (RequiresApproval.HasValue) eventEntity.RequiresApproval = RequiresApproval.Value;
        if (MeetingLink != null) eventEntity.MeetingLink = MeetingLink;
        if (Notes != null) eventEntity.Notes = Notes;

        eventEntity.UpdatedAt = DateTime.UtcNow;
    }
}