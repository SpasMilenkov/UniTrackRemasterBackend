namespace UniTrackRemaster.Api.Dto.Teacher.Analytics;

/// <summary>
/// DTO for student attendance statistics
/// </summary>
public class StudentAttendanceStatsDto
{
    public Guid StudentId { get; set; }
    public Guid? SemesterId { get; set; }
    public int TotalAbsences { get; set; }
    public int ExcusedAbsences { get; set; }
    public int UnexcusedAbsences { get; set; }
    public decimal AttendanceRate { get; set; } // Percentage
    public int TotalSchoolDays { get; set; }
    public int AttendedDays { get; set; }
}