using System;
using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Events;

namespace UniTrackRemaster.Api.Dto.Event;

public record UpdateParticipantStatusDto(
     [Required] ParticipantStatus Status,
     string? ResponseNote)
{
    public void ApplyToEntity(Participant participant)
    {
        participant.Status = Status;
        participant.ResponseNote = ResponseNote;
        participant.ResponseDate = DateTime.UtcNow;
        participant.UpdatedAt = DateTime.UtcNow;
    }
}
