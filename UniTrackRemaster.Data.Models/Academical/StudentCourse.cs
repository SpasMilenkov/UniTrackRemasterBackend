using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Academical;

public class StudentCourse : BaseEntity
{
    public Guid Id { get; init; }
    public Guid StudentId { get; set; }
    public Student Student { get; set; }
    public Guid CourseId { get; set; }
    public Course Course { get; set; }
    public EnrollmentStatus Status { get; set; }
    public DateTime EnrollmentDate { get; set; }
}