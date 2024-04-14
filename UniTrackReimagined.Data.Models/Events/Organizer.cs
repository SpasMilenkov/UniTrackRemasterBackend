using UniTrackReimagined.Data.Models.Users;

namespace UniTrackReimagined.Data.Models.Events;

public class Organizer
{
    #region Properties

    public Guid Id { get; set; }


    #endregion
    #region NavigationProperties

    public ICollection<Event>? OrganizedEvents { get; set; }
    public required Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }
    #endregion
}