using System;
using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Events;

namespace UniTrackRemaster.Api.Dto.Event;


public record CreateParticipantDto(
    [Required] Guid EventId,
    [Required] Guid UserId,
    bool IsRequired)
{
    public Participant ToEntity() => new()
    {
        Id = Guid.NewGuid(),
        EventId = EventId,
        UserId = UserId,
        Status = ParticipantStatus.Invited,
        IsRequired = IsRequired,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}
