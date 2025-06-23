namespace UniTrackRemaster.Api.Dto.Teacher.Analytics;

public class SubjectExcusedUnexcusedDto
{
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = "Unknown Subject";
    public int ExcusedCount { get; set; }
    public int UnexcusedCount { get; set; }
    public int Total { get; set; }
    public decimal ExcusedPercentage { get; set; }
    public decimal UnexcusedPercentage { get; set; }
}