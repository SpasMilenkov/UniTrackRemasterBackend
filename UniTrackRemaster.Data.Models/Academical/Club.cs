using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Organizations;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Academical;

public class Club : BaseEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string MeetingSchedule { get; set; }
    public int MaxMembers { get; set; }
    public ClubType Type { get; set; }
    
    public Guid? TeacherSupervisorId { get; set; }
    public Teacher TeacherSupervisor { get; set; }
    
    public Guid InstitutionId { get; set; }
    public Institution Institution { get; set; }
    
    public IList<ClubMembership> Memberships { get; set; }
    public IList<ClubEvent> Events { get; set; }
}
