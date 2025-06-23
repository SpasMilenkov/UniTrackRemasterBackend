using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Analytics;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Events;
using UniTrackRemaster.Data.Models.Organizations;
namespace UniTrackRemaster.Data.Models.Users;

public class Student : BaseEntity
{
    #region Properties
    //PrimaryKey
    public required Guid Id { get; set; }
    public required bool IsSchoolStudent { get; set; }
    public ProfileStatus Status { get; set; } = ProfileStatus.Pending;
    #endregion

    #region NavigationProperties
    //User relation
    public required Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public IList<Parent> Parents { get; set; } = new List<Parent>();

    //Events relation
    public IList<Event>? Events { get; set; }

    //School relation
    public Guid? SchoolId { get; set; }
    public School? School { get; set; }

    //University relation
    public Guid? UniversityId { get; set; }
    public University? University { get; set; }

    //Grade relation
    public Guid GradeId { get; set; }
    public Grade? Grade { get; set; }

    //Major relation (for university students)
    public Guid? MajorId { get; set; }
    public Major? Major { get; set; }

    //Mark relation
    public IList<Mark> Marks { get; set; }

    //Report relation
    public Guid? PersonalReportId { get; set; }

    public IList<StudentElective> Electives { get; set; } = new List<StudentElective>();
    public IList<Absence> AttendanceRecords { get; set; }
    #endregion
}