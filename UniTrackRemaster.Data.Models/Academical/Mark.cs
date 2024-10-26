using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Academical;

public class Mark
{
    #region Properties

    public Guid Id { get; set; }
    public decimal Value { get; set; }
    public required string Topic { get; set; }
    public string? Description { get; set; }

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