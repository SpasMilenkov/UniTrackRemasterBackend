namespace UniTrackRemaster.Api.Dto.Teacher.Analytics;


/// <summary>
///  Parameters for filtering attendance statistics with semester support
/// </summary>
public class StatisticsFilterParams
{
    /// <summary>Start date for filtering (optional - defaults to semester start)</summary>
    public DateTime? FromDate { get; set; }

    /// <summary>End date for filtering (optional - defaults to semester end or current date)</summary>
    public DateTime? ToDate { get; set; }

    /// <summary>Grade ID for filtering by specific class</summary>
    public Guid? GradeId { get; set; }

    /// <summary>Subject ID for filtering by specific subject</summary>
    public Guid? SubjectId { get; set; }

    /// <summary>Semester ID for filtering (optional - defaults to current active semester)</summary>
    public Guid? SemesterId { get; set; }
}
