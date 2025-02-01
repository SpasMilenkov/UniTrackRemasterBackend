using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Academical;


public class ElectiveSubject : BaseEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public int MaxStudents { get; set; }
    public ElectiveType Type { get; set; }
    
    public Guid TeacherId { get; set; }
    public Teacher Teacher { get; set; }
    
    public Guid? GradeId { get; set; }
    public Grade Grade { get; set; }
    
    public IList<StudentElective> StudentElectives { get; set; }
}