using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Request;

public record UpdateAttendanceDto(
    AttendanceStatus? Status,
    string? Reason,
    bool? IsExcused);