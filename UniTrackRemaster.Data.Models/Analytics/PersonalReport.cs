using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Analytics;

public class PersonalReport : BaseEntity
{
    public Guid Id { get; set; }
    public required string AcademicYear { get; set; }
    public decimal GPA { get; set; }
    public int TotalCredits { get; set; }
    public int AttendanceRate { get; set; }
    
    public Guid StudentId { get; set; }
    public Student Student { get; set; }
    public IList<ReportEntry> Entries { get; set; }
}
