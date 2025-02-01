using UniTrackRemaster.Data.Models.JunctionEntities;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Academical;

public class Grade: BaseEntity
{
    #region Properties

    public Guid Id { get; set; }
    public required string Name { get; set; }
    #endregion
    #region NavigationProperties
    
    public SubjectGradeTeacher? SubjectGradeTeacher { get; set; }
    public Guid HomeRoomTeacherId { get; set; }
    public IList<ElectiveSubject> ElectiveSubjects { get; set; }
    public HomeRoomTeacher? HomeRoomTeacher { get; set; }
    public IList<Student>? Students { get; set; }
    #endregion
}