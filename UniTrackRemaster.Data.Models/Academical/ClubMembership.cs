using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Academical;

public class ClubMembership : BaseEntity
{
    public Guid Id { get; set; }
    public Guid ClubId { get; set; }
    public Club Club { get; set; }
    
    public Guid StudentId { get; set; }
    public Student Student { get; set; }
    
    public ClubRole Role { get; set; }
    public DateTime JoinDate { get; set; }
    public MembershipStatus Status { get; set; }
}
