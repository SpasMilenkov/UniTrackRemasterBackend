using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Academical;

public class StudentElective : BaseEntity
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Student Student { get; set; }
    public Guid ElectiveSubjectId { get; set; }
    public ElectiveSubject ElectiveSubject { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public ElectiveStatus Status { get; set; }
}
