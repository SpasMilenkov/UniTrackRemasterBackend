using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Data.Models.Users;

public class Parent : BaseEntity
{
    #region Properties
    public required Guid Id { get; set; }
    public string? Occupation { get; set; }
    public string? EmergencyContact { get; set; }
    public ProfileStatus Status { get; set; } = ProfileStatus.Pending;
    public string? Notes { get; set; }
    #endregion
    
    #region NavigationProperties
    public required Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    public required IList<Student> Children { get; set; } = new List<Student>();
    #endregion
}