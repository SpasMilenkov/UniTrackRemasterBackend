
using UniTrackRemaster.Data.Models.JunctionEntities;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Academical;

public class Major : BaseEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public string ShortDescription { get; set; }
    public string DetailedDescription { get; set; }
    public int RequiredCredits { get; set; }
    public int DurationInYears { get; set; }
    public string CareerOpportunities { get; set; }
    public string AdmissionRequirements { get; set; }
    
    public Guid FacultyId { get; set; }
    public Faculty Faculty { get; set; }
    
    public IList<Course> Courses { get; set; }
    public IList<Student> Students { get; set; }
    public IList<SubjectGradeTeacher> SubjectGradeTeachers { get; set; }
}