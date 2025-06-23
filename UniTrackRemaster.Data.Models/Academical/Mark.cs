using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Academical;

public class Mark: BaseEntity
{
    #region Properties

    public Guid Id { get; set; }
    public decimal Value { get; set; }
    public required string Topic { get; set; }
    public string? Description { get; set; }
    public required MarkType Type { get; set; }
    public Guid? SemesterId { get; set; }
    public Semester? Semester { get; set; }

    #endregion
    #region NavigationProperties

    public required Guid SubjectId { get; set; }
    public Subject? Subject { get; set; }
    public required Guid TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
    public required Guid StudentId { get; set; }
    public Student? Student { get; set; }

    #endregion
}