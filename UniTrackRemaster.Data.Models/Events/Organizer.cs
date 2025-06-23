using UniTrackRemaster.Data.Models.Organizations;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Events;

public class Organizer : BaseEntity
{
    #region Properties
    public Guid Id { get; set; }
    public string? Department { get; set; }
    public string? Role { get; set; }
    public bool CanCreatePublicEvents { get; set; }
    #endregion

    #region NavigationProperties
    public IList<Event>? OrganizedEvents { get; set; }
    public required Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public Guid? InstitutionId { get; set; }
    public Institution? Institution { get; set; }
    #endregion
}