using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.Teacher.Analytics;


/// <summary>
///  Parameters for filtering at-risk students with semester support
/// </summary>
public class AtRiskStudentsFilterParams
{
    /// <summary>Grade ID for filtering by specific class</summary>
    public Guid? GradeId { get; set; }

    /// <summary>Subject ID for filtering by specific subject</summary>
    public Guid? SubjectId { get; set; }

    /// <summary>Start date for filtering (optional - defaults to semester start)</summary>
    public DateTime? FromDate { get; set; }

    /// <summary>End date for filtering (optional - defaults to semester end or current date)</summary>
    public DateTime? ToDate { get; set; }

    /// <summary>Semester ID for filtering (optional - defaults to current active semester)</summary>
    public Guid? SemesterId { get; set; }

    /// <summary>Threshold percentage for high risk classification (default: 20%)</summary>
    [Range(1, 100, ErrorMessage = "High risk threshold must be between 1 and 100")]
    public int? HighRiskThreshold { get; set; } = 20;

    /// <summary>Threshold percentage for medium risk classification (default: 10%)</summary>
    [Range(1, 100, ErrorMessage = "Medium risk threshold must be between 1 and 100")]
    public int? MediumRiskThreshold { get; set; } = 10;

    /// <summary>Total number of class days in the semester for calculating absence rates (optional - auto-calculated)</summary>
    [Range(1, 500, ErrorMessage = "Total class days must be between 1 and 500")]
    public int? TotalClassDays { get; set; }
}