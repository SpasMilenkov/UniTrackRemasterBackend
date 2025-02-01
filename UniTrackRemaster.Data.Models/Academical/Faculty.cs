using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Data.Models.Academical;

public class Faculty : BaseEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public required string ShortDescription { get; set; }
    public required string DetailedDescription { get; set; }
    public string? Website { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public required FacultyStatus Status { get; set; }
    
    public required Guid UniversityId { get; set; }
    public University University { get; set; }
    
    public IList<Major>? Majors { get; set; }
    public IList<Department>? Departments { get; set; }
}
