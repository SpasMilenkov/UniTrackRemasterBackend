namespace UniTrackRemaster.Api.Dto.Metrics;

public record ActivityStatisticsDto(
    int TotalAttendances,
    int TotalMarks,
    int TotalApplications,
    Dictionary<string, int> AttendanceByStatus,
    Dictionary<string, int> ApplicationsByStatus
);