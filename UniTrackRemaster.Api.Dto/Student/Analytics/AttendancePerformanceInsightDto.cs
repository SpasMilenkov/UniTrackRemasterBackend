namespace UniTrackRemaster.Api.Dto.Student.Analytics;


public record AttendancePerformanceInsightDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; }
    public string Period { get; set; }
    public Guid? SemesterId { get; set; }
    public string SemesterName { get; set; }
    public double OverallStudentAttendanceRate { get; set; }
    public decimal OverallStudentAverageScore { get; set; }
    public string OverallStudentGrade { get; set; }
    public double OverallClassAttendanceRate { get; set; }
    public decimal OverallClassAverageScore { get; set; }
    public double OverallCorrelation { get; set; }
    public int AttendancePerformanceClassRank { get; set; }
    public List<SubjectAttendanceInsightDto> SubjectInsights { get; set; }
    public string PrimaryAreaForImprovement { get; set; }
    public double EstimatedGpaImpact { get; set; }
}