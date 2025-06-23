namespace UniTrackRemaster.Api.Dto.Teacher.Analytics;

/// <summary>
/// DTO for subject attendance summary
/// </summary>
public class SubjectAttendanceSummaryDto
{
    public Guid SubjectId { get; set; }
    public Guid? SemesterId { get; set; }
    public int TotalStudents { get; set; }
    public int StudentsWithAbsences { get; set; }
    public int TotalAbsences { get; set; }
    public int ExcusedAbsences { get; set; }
    public int UnexcusedAbsences { get; set; }
    public decimal AverageAbsencesPerStudent { get; set; }
}