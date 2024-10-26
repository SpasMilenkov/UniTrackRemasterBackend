using UniTrackRemaster.Data.Models.JunctionEntities;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Academical;

public class Grade
{
    #region Properties

    public Guid Id { get; set; }
    public required string Name { get; set; }
    #endregion
    #region NavigationProperties
    
    public SubjectGradeTeacher? SubjectGradeTeacher { get; set; }
    public required ICollection<Student> Students { get; set; }
    #endregion
}