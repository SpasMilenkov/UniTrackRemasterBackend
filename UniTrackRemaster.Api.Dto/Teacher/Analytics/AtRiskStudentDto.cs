namespace UniTrackRemaster.Api.Dto.Teacher.Analytics;

public class AtRiskStudentDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid GradeId { get; set; }
    public string GradeName { get; set; } = string.Empty;
    public int TotalAbsences { get; set; }
    public decimal AbsenceRate { get; set; }
    public string RiskLevel { get; set; } = "none"; // "high", "medium", "none"
    public List<string> RecentPattern { get; set; } = new(); // Last 5 attendance entries
}
