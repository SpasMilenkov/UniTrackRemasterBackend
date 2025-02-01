using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Data.Models.Academical;

public class Course : BaseEntity
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; }
    public int Credits { get; set; }
    public required CourseType Type { get; set; }
    
    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; }
    
    public Guid? MajorId { get; set; }
    public Major? Major { get; set; }
    
    public Guid SemesterId { get; set; }
    public Semester Semester { get; set; }
    
    public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();
    public ICollection<CourseAssignment> Assignments { get; set; } = new List<CourseAssignment>();
}