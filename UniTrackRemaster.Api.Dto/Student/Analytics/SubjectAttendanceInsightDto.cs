namespace UniTrackRemaster.Api.Dto.Student.Analytics;

public record SubjectAttendanceInsightDto
{
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; }
    public Guid? SemesterId { get; set; }
    public string SemesterName { get; set; }
    public double StudentAttendanceRate { get; set; }
    public decimal StudentAverageScore { get; set; }
    public double ClassAvgAttendanceRate { get; set; }
    public decimal ClassAvgScore { get; set; }
    public double AttendanceScoreCorrelation { get; set; }
    public decimal ImprovementPotential { get; set; }
    public double OptimalAttendanceTarget { get; set; }
    public string PerformanceImpact { get; set; }
    public string PersonalizedRecommendation { get; set; }
}
