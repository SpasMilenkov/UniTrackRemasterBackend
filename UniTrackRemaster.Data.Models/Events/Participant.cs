using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Events;

public class Participant: BaseEntity
{
    #region Properties

    public required Guid Id { get; set; }

    #endregion
    #region NavigationProperties

    public required Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    #endregion
}