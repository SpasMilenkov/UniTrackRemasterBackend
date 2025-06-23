namespace UniTrackRemaster.Api.Dto.Teacher.Analytics;

/// <summary>
///  Attendance statistics with semester context
/// </summary>
public class AttendanceStatisticsDto
{
    /// <summary>Total number of students in the analysis</summary>
    public int TotalStudents { get; set; }

    /// <summary>Total number of absences recorded</summary>
    public int TotalAbsences { get; set; }

    /// <summary>Overall attendance rate as a percentage</summary>
    public decimal AttendanceRate { get; set; }

    /// <summary>Breakdown of absences by status (Present, Absent, Late, etc.)</summary>
    public Dictionary<string, int> AbsencesByStatus { get; set; } = new();

    /// <summary>Breakdown of absences by subject</summary>
    public Dictionary<string, int> AbsencesBySubject { get; set; } = new();

    /// <summary>Breakdown of absences by grade/class</summary>
    public Dictionary<string, int> AbsencesByGrade { get; set; } = new();

    /// <summary>Daily trend data for visualization</summary>
    public Dictionary<string, int> TrendByDay { get; set; } = new();

    /// <summary>Weekly trend data for visualization</summary>
    public Dictionary<string, int> TrendByWeek { get; set; } = new();

    /// <summary>Monthly trend data for visualization</summary>
    public Dictionary<string, int> TrendByMonth { get; set; } = new();

    /// <summary>ID of the semester this data represents</summary>
    public Guid? SemesterId { get; set; }

    /// <summary>Name of the semester this data represents</summary>
    public string? SemesterName { get; set; }
}