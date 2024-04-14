using UniTrackReimagined.Data.Models.Users;

namespace UniTrackReimagined.Data.Models.Analytics;

public class Recommendation
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