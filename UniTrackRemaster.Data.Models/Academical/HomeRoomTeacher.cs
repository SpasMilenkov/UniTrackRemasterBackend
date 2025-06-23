using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Academical;

public class HomeRoomTeacher : BaseEntity
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public Teacher Teacher { get; set; }
    public Guid GradeId { get; set; }
    public Grade Grade { get; set; }
}
