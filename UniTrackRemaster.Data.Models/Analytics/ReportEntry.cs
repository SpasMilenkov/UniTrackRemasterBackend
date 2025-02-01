namespace UniTrackRemaster.Data.Models.Analytics;

public class ReportEntry : BaseEntity
{
    public Guid Id { get; set; }
    public string Category { get; set; }
    public string MetricName { get; set; }
    public string Value { get; set; }
    public DateTime Date { get; set; }
    
    public Guid PersonalReportId { get; set; }
    public PersonalReport PersonalReport { get; set; }
}
