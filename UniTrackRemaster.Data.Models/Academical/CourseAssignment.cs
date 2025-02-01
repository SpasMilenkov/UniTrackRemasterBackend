using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Data.Models.Academical;

public class CourseAssignment : BaseEntity
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string Description { get; set; }
    public required DateTime DueDate { get; set; }
    public required decimal MaxScore { get; set; }
    public decimal Weight { get; set; }
    public AssignmentType Type { get; set; }
    
    public Guid CourseId { get; set; }
    public Course Course { get; set; }
    public IList<StudentAssignment> StudentAssignments { get; set; }
}