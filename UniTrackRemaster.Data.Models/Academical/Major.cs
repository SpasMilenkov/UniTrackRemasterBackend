
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Academical;

public class Major : BaseEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public required string ShortDescription { get; set; }
    public required string DetailedDescription { get; set; }
    public int RequiredCredits { get; set; }
    public int DurationInYears { get; set; }
    public required string CareerOpportunities { get; set; }
    public required string AdmissionRequirements { get; set; }
    
    public Guid FacultyId { get; set; }
    public Faculty? Faculty { get; set; }
    
    public IList<Subject>? Subjects { get; set; }
    public IList<Student>? Students { get; set; }
}