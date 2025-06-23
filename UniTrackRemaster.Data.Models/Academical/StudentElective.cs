using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Academical;


public class StudentElective
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Student Student { get; set; }
    
    public Guid SubjectId { get; set; } 
    public Subject Subject { get; set; }
    
    public DateTime EnrollmentDate { get; set; }
    public ElectiveStatus Status { get; set; }
}