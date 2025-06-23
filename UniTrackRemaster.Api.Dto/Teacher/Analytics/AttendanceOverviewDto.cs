namespace UniTrackRemaster.Api.Dto.Teacher.Analytics;


/// <summary>
///  Attendance overview response with semester context
/// </summary>
public class AttendanceOverviewDto
{
    /// <summary>Total number of absences in the filtered period</summary>
    public int TotalAbsences { get; set; }

    /// <summary>Number of recent absences (last 7 days)</summary>
    public int RecentAbsences { get; set; }

    /// <summary>Breakdown of absences by status (Present, Absent, Late, etc.)</summary>
    public Dictionary<string, int> AbsencesByStatus { get; set; } = new();    

    /// <summary>Daily absence trend data for visualization</summary>
    public Dictionary<string, int> DailyAbsenceTrend { get; set; } = new();

    /// <summary>ID of the semester this data represents</summary>
    public Guid? SemesterId { get; set; }

    /// <summary>Name of the semester this data represents</summary>
    public string? SemesterName { get; set; }
}