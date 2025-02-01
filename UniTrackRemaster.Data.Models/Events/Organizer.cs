using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Events;

public class Organizer: BaseEntity
{
    #region Properties

    public Guid Id { get; set; }


    #endregion
    #region NavigationProperties

    public IList<Event>? OrganizedEvents { get; set; }
    public required Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }
    #endregion
}