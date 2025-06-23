using System;
using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Events;

namespace UniTrackRemaster.Api.Dto.Event;

public record CreateOrganizerDto(
    [Required] Guid UserId,
    string? Department,
    string? Role,
    bool CanCreatePublicEvents,
    Guid? InstitutionId)
{
    public Organizer ToEntity() => new()
    {
        Id = Guid.NewGuid(),
        UserId = UserId,
        Department = Department,
        Role = Role,
        CanCreatePublicEvents = CanCreatePublicEvents,
        InstitutionId = InstitutionId,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}
