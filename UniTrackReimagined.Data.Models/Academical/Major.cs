
using UniTrackReimagined.Data.Models.JunctionEntities;

namespace UniTrackReimagined.Data.Models.Academical;

public class Major
{
    #region Properties

    public Guid Id { get; set; }
    public required string Name { get; set; }
    #endregion
    #region NavigationProperties
    
    public ICollection<SubjectGradeTeacher> SubjectGradeTeachers { get; set; }

    #endregion
}