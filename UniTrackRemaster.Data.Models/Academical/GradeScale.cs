namespace UniTrackRemaster.Data.Models.Academical;

/// <summary>
/// Represents individual grade thresholds in a grading system
/// </summary>
public class GradeScale : BaseEntity
{
    public Guid Id { get; set; }
    public string Grade { get; set; }
    public string Description { get; set; }
    public decimal MinimumScore { get; set; }
    public decimal MaximumScore { get; set; }
    public double GpaValue { get; set; }

    // Navigation properties
    public Guid GradingSystemId { get; set; }
    public virtual GradingSystem GradingSystem { get; set; }
}
