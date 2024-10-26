namespace UniTrackRemaster.Api.Dto.Request;

public class UpdateSchoolDto
{
    public string? Name { get; set; }
    public required Guid SchoolId { get; set; }
    public string? Description { get; set; }
    public ICollection<Guid>? StudentIds { get; set; }
    public ICollection<Guid>? TeacherIds { get; set; }
    public ICollection<Guid>? MajorIds { get; set; }
}