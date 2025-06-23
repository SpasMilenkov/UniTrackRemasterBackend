using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Data.Models.Users;

public class Admin : BaseEntity
{
    #region Properties
    public required Guid Id { get; set; }
    public required string Position { get; set; }
    public string? Department { get; set; }
    public DateTime StartDate { get; set; }
    public AdminRole Role { get; set; }
        public string? Notes { get; set; }
    public ProfileStatus Status { get; set; } = ProfileStatus.Pending;
    #endregion

    #region NavigationProperties
    public required Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public required Guid InstitutionId { get; set; }
    public Institution? Institution { get; set; }
    #endregion
}