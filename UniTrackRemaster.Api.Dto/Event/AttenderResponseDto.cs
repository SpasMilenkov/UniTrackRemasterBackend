using System;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Events;

namespace UniTrackRemaster.Api.Dto.Event;

public record AttenderResponseDto(
    Guid Id,
    Guid UserId,
    string UserName,
    AttendanceStatus AttendanceStatus,
    DateTime? CheckInTime,
    DateTime? CheckOutTime,
    string? AttendanceNotes,
    DateTime CreatedAt)
{
    public static AttenderResponseDto FromEntity(Attender attender) => new(
        attender.Id,
        attender.UserId,
        attender.User != null ? $"{attender.User.FirstName} {attender.User.LastName}" : "",
        attender.AttendanceStatus,
        attender.CheckInTime,
        attender.CheckOutTime,
        attender.AttendanceNotes,
        attender.CreatedAt
    );
}