using UniTrackRemaster.Data.Models.JunctionEntities;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Academical;

public class Subject: BaseEntity
{
    #region Properties

    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string ShortDescription { get; set; }
    public string? DetailedDescription { get; set; }
    
    #endregion
    #region NavigationProperties

    public ICollection<Grade> Grades { get; set; }
    public ICollection<Teacher> Teachers { get; set; }
    #endregion
}