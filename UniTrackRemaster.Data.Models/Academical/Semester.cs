namespace UniTrackRemaster.Data.Models.Academical;

public class Semester : BaseEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    
    public Guid AcademicYearId { get; set; }
    public AcademicYear AcademicYear { get; set; }
    public IList<Course> Courses { get; set; }
}
