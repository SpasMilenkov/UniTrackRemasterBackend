using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Events;

public class Participant : BaseEntity
{
    #region Properties
    public required Guid Id { get; set; }
    public ParticipantStatus Status { get; set; } = ParticipantStatus.Invited;
    public DateTime? ResponseDate { get; set; }
    public string? ResponseNote { get; set; }
    public bool IsRequired { get; set; }
    #endregion

    #region NavigationProperties
    public required Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public required Guid EventId { get; set; }
    public Event? Event { get; set; }
    #endregion
}
