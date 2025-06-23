using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Data.Models.Academical;

/// <summary>
/// Base entity for institutional grading systems
/// </summary>
public class GradingSystem : BaseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public GradingSystemType Type { get; set; }
    public bool IsDefault { get; set; }
    public decimal MinimumPassingScore { get; set; }
    public decimal MaximumScore { get; set; }

    // Navigation properties
    public Guid InstitutionId { get; set; }
    public virtual Institution Institution { get; set; }

    // For customized grading systems
    public virtual ICollection<GradeScale> GradeScales { get; set; } = new List<GradeScale>();
}