using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Events;

public class Attender
{
    #region Properties

    public Guid Id { get; set; }

    #endregion
    #region NavigationProperties

    public required Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public ICollection<Event>? Events { get; set; }

    #endregion
}