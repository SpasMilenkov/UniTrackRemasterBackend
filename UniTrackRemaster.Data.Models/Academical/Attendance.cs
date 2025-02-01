using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Academical;

public class Attendance : BaseEntity
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public string Reason { get; set; }
    public bool IsExcused { get; set; }
    
    public Guid StudentId { get; set; }
    public Student Student { get; set; }
    
    public Guid? CourseId { get; set; }
    public Course Course { get; set; }
    
    public Guid? SubjectId { get; set; }
    public Subject Subject { get; set; }
}