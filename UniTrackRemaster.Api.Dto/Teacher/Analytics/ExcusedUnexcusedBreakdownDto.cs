namespace UniTrackRemaster.Api.Dto.Teacher.Analytics;


/// <summary>
///  Excused vs unexcused breakdown with semester context
/// </summary>
public class ExcusedUnexcusedBreakdownDto
{
    /// <summary>Total count of excused absences</summary>
    public int ExcusedCount { get; set; }

    /// <summary>Total count of unexcused absences</summary>
    public int UnexcusedCount { get; set; }

    /// <summary>Percentage of excused absences</summary>
    public decimal ExcusedPercentage { get; set; }

    /// <summary>Percentage of unexcused absences</summary>
    public decimal UnexcusedPercentage { get; set; }

    /// <summary>Breakdown by subject with excused/unexcused counts</summary>
    public List<SubjectExcusedUnexcusedDto> SubjectBreakdown { get; set; } = new();

    /// <summary>ID of the semester this data represents</summary>
    public Guid? SemesterId { get; set; }

    /// <summary>Name of the semester this data represents</summary>
    public string? SemesterName { get; set; }
}
