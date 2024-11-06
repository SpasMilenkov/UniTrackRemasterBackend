using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Analytics;

public class Recommendation: BaseEntity
{
    #region Properties

    public Guid Id { get; set; }
    public required string Topic { get; set; }
    public required string SourceLink { get; set; }
    public required DateTime Date { get; set; }
    #endregion
    #region NavigationProperties

    public required Guid UserId { get; set; }
    public required ApplicationUser User { get; set; }
    #endregion
}