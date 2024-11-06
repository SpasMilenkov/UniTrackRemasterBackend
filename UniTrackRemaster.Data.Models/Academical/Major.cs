
using UniTrackRemaster.Data.Models.JunctionEntities;

namespace UniTrackRemaster.Data.Models.Academical;

public class Major: BaseEntity
{
    #region Properties

    public Guid Id { get; set; }
    public required string Name { get; set; }
    #endregion
    #region NavigationProperties
    
    public ICollection<SubjectGradeTeacher> SubjectGradeTeachers { get; set; }

    #endregion
}