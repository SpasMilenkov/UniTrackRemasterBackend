using UniTrackReimagined.Data.Models.Users;

namespace UniTrackReimagined.Data.Models.Events;

public class Participant
{
    #region Properties

    public required Guid Id { get; set; }

    #endregion
    #region NavigationProperties

    public required Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    #endregion
}