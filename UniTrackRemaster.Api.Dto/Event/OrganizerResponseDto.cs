using System;
using UniTrackRemaster.Data.Models.Events;

namespace UniTrackRemaster.Api.Dto.Event;


public record OrganizerResponseDto(
    Guid Id,
    Guid UserId,
    string UserName,
    string? Department,
    string? Role,
    bool CanCreatePublicEvents,
    Guid? InstitutionId,
    string? InstitutionName,
    int OrganizedEventsCount,
    DateTime CreatedAt)
{
    public static OrganizerResponseDto FromEntity(Organizer organizer) => new(
        organizer.Id,
        organizer.UserId,
        organizer.User != null ? $"{organizer.User.FirstName} {organizer.User.LastName}" : "",
        organizer.Department,
        organizer.Role,
        organizer.CanCreatePublicEvents,
        organizer.InstitutionId,
        organizer.Institution?.Name,
        organizer.OrganizedEvents?.Count ?? 0,
        organizer.CreatedAt
    );
}
