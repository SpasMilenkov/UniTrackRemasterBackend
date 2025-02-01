using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Data.Models.Academical;

public class AcademicYear : BaseEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    
    public ICollection<Semester> Semesters { get; set; }
    public Guid InstitutionId { get; set; }
    public Institution Institution { get; set; }
}
