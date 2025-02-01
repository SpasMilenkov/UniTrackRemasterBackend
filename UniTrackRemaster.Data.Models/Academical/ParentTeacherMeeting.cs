using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Academical;

public class ParentTeacherMeeting : BaseEntity
{
    public Guid Id { get; set; }
    public DateTime ScheduledTime { get; set; }
    public string Notes { get; set; }
    public MeetingStatus Status { get; set; }
    
    public Guid HomeRoomTeacherId { get; set; }
    public HomeRoomTeacher HomeRoomTeacher { get; set; }
    
    public Guid ParentId { get; set; }
    public Parent Parent { get; set; }
}