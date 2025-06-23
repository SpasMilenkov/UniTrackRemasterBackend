using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.Teacher.Analytics;

/// <summary>
///  Parameters for filtering attendance overview data with semester support
/// </summary>
public class AttendanceFilterParams
{
    /// <summary>Start date for filtering attendance data (optional - defaults to semester start)</summary>
    public DateTime? FromDate { get; set; }

    /// <summary>End date for filtering attendance data (optional - defaults to semester end or current date)</summary>
    public DateTime? ToDate { get; set; }

    /// <summary>Grade ID for filtering by specific class</summary>
    public Guid? GradeId { get; set; }

    /// <summary>Subject ID for filtering by specific subject</summary>
    public Guid? SubjectId { get; set; }

    /// <summary>Semester ID for filtering (optional - defaults to current active semester)</summary>
    public Guid? SemesterId { get; set; }

    /// <summary>Number of days to include in trend analysis (default: 14)</summary>
    [Range(1, 365, ErrorMessage = "Days to show must be between 1 and 365")]
    public int? DaysToShow { get; set; } = 14;
}