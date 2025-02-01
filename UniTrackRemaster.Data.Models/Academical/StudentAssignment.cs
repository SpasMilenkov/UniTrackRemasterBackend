using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Academical;

public class StudentAssignment : BaseEntity
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Student Student { get; set; }
    public Guid AssignmentId { get; set; }
    public CourseAssignment Assignment { get; set; }
    public decimal? Score { get; set; }
    public string Feedback { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public AssignmentStatus Status { get; set; }
}