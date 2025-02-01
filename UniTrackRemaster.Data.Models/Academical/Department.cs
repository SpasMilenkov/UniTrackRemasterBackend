using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Academical;

public class Department : BaseEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public DepartmentType Type { get; set; }
    public DepartmentStatus Status { get; set; }
    
    public Guid FacultyId { get; set; }
    public Faculty Faculty { get; set; }
    
    public ICollection<Teacher>? Teachers { get; set; }
}
