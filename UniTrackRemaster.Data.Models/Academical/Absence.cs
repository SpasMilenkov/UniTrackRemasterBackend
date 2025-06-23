using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Academical;

public class Absence : BaseEntity
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public AbsenceStatus Status { get; set; }
    public string Reason { get; set; }
    public bool IsExcused { get; set; }
    
    public Guid? SemesterId { get; set; }
    public Semester? Semester { get; set; }
    
    public Guid StudentId { get; set; }
    public Student Student { get; set; }
    
    public Guid SubjectId { get; set; }
    public Subject? Subject { get; set; }

    public Guid TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
}