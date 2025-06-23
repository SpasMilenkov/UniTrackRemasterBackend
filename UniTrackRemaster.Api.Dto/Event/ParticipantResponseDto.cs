using System;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Events;

namespace UniTrackRemaster.Api.Dto.Event;

public record ParticipantResponseDto(
     Guid Id,
     Guid UserId,
     string UserName,
     string UserEmail,
     ParticipantStatus Status,
     DateTime? ResponseDate,
     string? ResponseNote,
     bool IsRequired,
     DateTime CreatedAt)
{
    public static ParticipantResponseDto FromEntity(Participant participant) => new(
        participant.Id,
        participant.UserId,
        participant.User != null ? $"{participant.User.FirstName} {participant.User.LastName}" : "",
        participant.User?.Email ?? "",
        participant.Status,
        participant.ResponseDate,
        participant.ResponseNote,
        participant.IsRequired,
        participant.CreatedAt
    );
}