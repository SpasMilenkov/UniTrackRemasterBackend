using UniTrackRemaster.Data.Models.Organizations;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Academical;

public class Grade : BaseEntity
{
    #region Properties

    public Guid Id { get; set; }
    public required string Name { get; set; }
    #endregion
    #region NavigationProperties

    public Guid HomeRoomTeacherId { get; set; }
    public Institution? Institution { get; set; }
    public Guid InstitutionId { get; set; }
    public AcademicYear? AcademicYear { get; set; }
    public Guid AcademicYearId { get; set; }
    public IList<ElectiveSubject>? ElectiveSubjects { get; set; }
    public HomeRoomTeacher? HomeRoomTeacher { get; set; }
    public IList<Student>? Students { get; set; }
    public IList<Teacher>? Teachers { get; set; }
    public IList<Subject>? Subjects { get; set; }
    #endregion
}